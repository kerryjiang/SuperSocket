using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SuperSocket;

#if NET6_0_OR_GREATER

public sealed class CancellationTokenSourcePool
{
    private const int MaxQueueSize = 1024;

    private readonly ConcurrentQueue<PooledCancellationTokenSource> _queue = new();
    private int _count;

    public static readonly CancellationTokenSourcePool Shared = new();
    
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
    /// A <see cref="CancellationTokenSource"/> with a back pointer to the pool it came from.
    /// Dispose will return it to the pool.
    /// </summary>
    public sealed class PooledCancellationTokenSource
        : CancellationTokenSource
    {
        private readonly CancellationTokenSourcePool _pool;

        public PooledCancellationTokenSource(CancellationTokenSourcePool pool)
        {
            _pool = pool;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            // If we failed to return to the pool then dispose
            if (!_pool.Return(this))
            {
                base.Dispose(disposing);
            }
        }
    }
}

#endif