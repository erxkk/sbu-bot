using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Kkommon
{
    /// <summary>
    ///     An event that allows asynchronous invocation.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event args to pass to the handlers.</typeparam>
    [PublicAPI]
    public sealed class AsyncEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private ErrorHandler? _asyncEventErrorHandler;
        private ImmutableArray<Handler> _handlers = ImmutableArray<Handler>.Empty;

        /// <summary>
        ///     Creates a new async event with no specified error handler.
        /// </summary>
        public AsyncEvent() { }

        /// <summary>
        ///     Creates a new async event with the specified error handler.
        /// </summary>
        /// <param name="asyncEventErrorHandler"></param>
        public AsyncEvent(ErrorHandler asyncEventErrorHandler)
            => _asyncEventErrorHandler = asyncEventErrorHandler;

        /// <summary>
        ///     Adds a new handler to the invocation list.
        /// </summary>
        /// <param name="asyncEventHandler">The handler to add.</param>
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        public void Add(Handler asyncEventHandler)
        {
            lock (this)
            {
                _handlers = _handlers.Add(asyncEventHandler);
            }
        }

        /// <summary>
        ///     Removes a handler from the invocation list.
        /// </summary>
        /// <param name="asyncEventHandler">The handler to remove.</param>
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        public void Remove(Handler asyncEventHandler)
        {
            lock (this)
            {
                _handlers = _handlers.Remove(asyncEventHandler);
            }
        }

        /// <summary>
        ///     Invokes the event asynchronously.
        /// </summary>
        /// <param name="args">The event args to pass to the handlers.</param>
        [CollectionAccess(CollectionAccessType.Read)]
        public async Task InvokeAsync(TEventArgs args)
        {
            ImmutableArray<Handler> copy;

            lock (this)
            {
                copy = _handlers;
            }

            foreach (Handler handler in copy)
            {
                try
                {
                    await handler.Invoke(args);
                }
                catch (Exception ex)
                {
                    if (_asyncEventErrorHandler is null)
                        throw;

                    await _asyncEventErrorHandler(args, ex);
                }
            }
        }

        /// <summary>
        ///     An asynchronous event handler delegate.
        /// </summary>
        public delegate ValueTask Handler(TEventArgs args);

        /// <summary>
        ///     An asynchronous event error handler delegate.
        /// </summary>
        public delegate ValueTask ErrorHandler(TEventArgs args, Exception exception);
    }
}