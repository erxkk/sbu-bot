using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Kkommon.Threading;

namespace Kkommon.Extensions.Semaphore
{
    /// <summary>
    ///     Useful extensions to enter a semaphore and automatically releasing it in case of an exception by use of a
    ///     <see cref="SemaphoreSlimSafeHandle" />.
    /// </summary>
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SemaphoreExtensions
    {
        /// <summary>
        ///     Blocks the current thread until it can enter the <see cref="SemaphoreSlim" />, while observing a
        ///     <see cref="CancellationToken" />.
        /// </summary>
        /// <param name="this">
        ///     The <see cref="SemaphoreSlim" /> to enter.
        /// </param>
        /// <param name="cancellationToken">
        ///     The <see cref="CancellationToken" /> token to observe.
        /// </param>
        /// <returns>The <see cref="SemaphoreSlimSafeHandle" /> to dispose.</returns>
        /// <exception cref="OperationCanceledException">
        ///     The <paramref name="cancellationToken" /> is canceled.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     The current instance has already been disposed.
        /// </exception>
        public static SemaphoreSlimSafeHandle Enter(
            this SemaphoreSlim @this,
            CancellationToken cancellationToken = default
        )
        {
            @this.Wait(cancellationToken);
            return new(@this);
        }

        /// <summary>
        ///     Asynchronously waits to enter the <see cref="SemaphoreSlim" />, while observing a
        ///     <see cref="CancellationToken" />.
        /// </summary>
        /// <param name="cancellationToken">
        ///     The <see cref="CancellationToken" /> token to observe.
        /// </param>
        /// <returns>The <see cref="SemaphoreSlimSafeHandle" /> to dispose.</returns>
        /// <exception cref="ObjectDisposedException">
        ///     The current instance has already been disposed.
        /// </exception>
        public static async Task<SemaphoreSlimSafeHandle> EnterAsync(
            this SemaphoreSlim @this,
            CancellationToken cancellationToken = default
        )
        {
            await @this.WaitAsync(cancellationToken);
            return new(@this);
        }
    }
}