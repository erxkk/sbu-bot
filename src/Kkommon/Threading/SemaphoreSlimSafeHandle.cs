using System;
using System.Threading;

using JetBrains.Annotations;

namespace Kkommon.Threading
{
    /// <summary>
    ///     A <see cref="IDisposable" /> handle that releases the semaphore on disposal.
    /// </summary>
    [PublicAPI]
    public readonly struct SemaphoreSlimSafeHandle : IDisposable
    {
        private readonly SemaphoreSlim? _semaphore;
        private readonly bool _isDisposed;

        internal SemaphoreSlimSafeHandle(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
            _isDisposed = false;
        }

        /// <summary>
        ///     Releases the underlying semaphore once.
        /// </summary>
        /// <exception cref="InvalidOperationException">The semaphore is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">The semaphore is already released by this handle.</exception>
        public void Dispose()
        {
            if (_semaphore is null)
                throw new InvalidOperationException(
                    $"No {nameof(SemaphoreSlim)} set, do not use default ctor for {nameof(SemaphoreSlimSafeHandle)}"
                );

            if (_isDisposed)
                throw new InvalidOperationException(
                    "The Semaphore was already released by this handle."
                );

            _semaphore.Release();
        }
    }
}