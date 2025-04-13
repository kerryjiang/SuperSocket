using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SuperSocket;

#if NET6_0_OR_GREATER

/// <summary>
/// Provides a pool for reusing <see cref="CancellationTokenSource"/> instances to reduce memory allocations.
/// </summary>
public sealed class CancellationTokenSourcePool
{
    private const int MaxQueueSize = 1024;

    private readonly ConcurrentQueue<PooledCancellationTokenSource> _queue = new();
    private int _count;

    /// <summary>
    /// Gets a shared instance of the <see cref="CancellationTokenSourcePool"/>.
    /// </summary>
    public static readonly CancellationTokenSourcePool Shared = new();

    /// <summary>
    /// Rents a <see cref="PooledCancellationTokenSource"/> from the pool.
    /// </summary>
    /// <returns>A pooled <see cref="PooledCancellationTokenSource"/>.</returns>
    public PooledCancellationTokenSource Rent()
    {
        if (_queue.TryDequeue(out var cts))
        {
            Interlocked.Decrement(ref _count);
            cts.CancelAfter(Timeout.Infinite);
            return cts;
        }

        return new PooledCancellationTokenSource(this);
    }

    /// <summary>
    /// Rents a <see cref="PooledCancellationTokenSource"/> from the pool and sets a cancellation delay.
    /// </summary>
    /// <param name="delay">The delay after which the token will be canceled.</param>
    /// <returns>A pooled <see cref="PooledCancellationTokenSource"/>.</returns>
    public PooledCancellationTokenSource Rent(TimeSpan delay)
    {
        var token = Rent();
        token.CancelAfter(delay);
        return token;
    }

    private bool Return(PooledCancellationTokenSource cts)
    {
        if (Interlocked.Increment(ref _count) > MaxQueueSize || !cts.TryReset())
        {
            Interlocked.Decrement(ref _count);
            return false;
        }

        _queue.Enqueue(cts);
        return true;
    }

    /// <summary>
    /// Represents a <see cref="CancellationTokenSource"/> with a back pointer to the pool it came from.
    /// </summary>
    public sealed class PooledCancellationTokenSource : CancellationTokenSource
    {
        private readonly CancellationTokenSourcePool _pool;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledCancellationTokenSource"/> class with the specified pool.
        /// </summary>
        /// <param name="pool">The pool to which this instance belongs.</param>
        public PooledCancellationTokenSource(CancellationTokenSourcePool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Disposes the <see cref="PooledCancellationTokenSource"/> and returns it to the pool if possible.
        /// </summary>
        /// <param name="disposing">A value indicating whether the object is being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            // If we failed to return to the pool, then dispose
            if (!_pool.Return(this))
            {
                base.Dispose(disposing);
            }
        }
    }
}

#endif