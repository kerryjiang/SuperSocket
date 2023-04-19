using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SuperSocket.IOCPTcpChannelCreatorFactory.Internal;

/// <summary>
/// Wraps an array allocated in the pinned object heap in a reusable block of managed memory
/// </summary>
internal sealed class MemoryPoolBlock : IMemoryOwner<byte>, IDisposable
{
    internal MemoryPoolBlock(PinnedBlockMemoryPool pool, int length)
    {
        this.Pool = pool;
        byte[] array = GC.AllocateUninitializedArray<byte>(length, true);
        this.Memory = MemoryMarshal.CreateFromPinnedArray<byte>(array, 0, array.Length);
    }

    /// <summary>
    /// Back-reference to the memory pool which this block was allocated from. It may only be returned to this pool.
    /// </summary>
    public PinnedBlockMemoryPool Pool { get; }

    public System.Memory<byte> Memory { get; }

    public void Dispose() => this.Pool.Return(this);
}

/// <summary>
/// Used to allocate and distribute re-usable blocks of memory.
/// </summary>
internal sealed class PinnedBlockMemoryPool : MemoryPool<byte>
{
    /// <summary>
    /// The size of a block. 4096 is chosen because most operating systems use 4k pages.
    /// </summary>
    private const int _blockSize = 4096;

#nullable disable
    /// <summary>
    /// Thread-safe collection of blocks which are currently in the pool. A slab will pre-allocate all of the block tracking objects
    /// and add them to this collection. When memory is requested it is taken from here first, and when it is returned it is re-added.
    /// </summary>
    private readonly ConcurrentQueue<MemoryPoolBlock> _blocks = new();
    /// <summary>This is part of implementing the IDisposable pattern.</summary>
    private bool _isDisposed;
    private readonly object _disposeSync = new();
    /// <summary>
    /// This default value passed in to Rent to use the default value for the pool.
    /// </summary>
    private const int AnySize = -1;

    /// <summary>
    /// Max allocation block size for pooled blocks,
    /// larger values can be leased but they will be disposed after use rather than returned to the pool.
    /// </summary>
    public override int MaxBufferSize { get; } = 4096;

    /// <summary>
    /// The size of a block. 4096 is chosen because most operating systems use 4k pages.
    /// </summary>
    public static int BlockSize => 4096;


#nullable enable
    public override IMemoryOwner<byte> Rent(int size = -1)
    {
        if (size > 4096)
            throw new Exception();
        if (_isDisposed)
            throw new Exception();

        return _blocks.TryDequeue(out var result) ? result : new MemoryPoolBlock(this, BlockSize);
    }

    /// <summary>
    /// Called to return a block to the pool. Once Return has been called the memory no longer belongs to the caller, and
    /// Very Bad Things will happen if the memory is read of modified subsequently. If a caller fails to call Return and the
    /// block tracking object is garbage collected, the block tracking object's finalizer will automatically re-create and return
    /// a new tracking object into the pool. This will only happen if there is a bug in the server, however it is necessary to avoid
    /// leaving "dead zones" in the slab due to lost block tracking objects.
    /// </summary>
    /// <param name="block">The block to return. It must have been acquired by calling Lease on the same memory pool instance.</param>
    internal void Return(MemoryPoolBlock block)
    {
        if (this._isDisposed)
            return;
        this._blocks.Enqueue(block);
    }

    protected override void Dispose(bool disposing)
    {
        if (this._isDisposed)
            return;
        lock (this._disposeSync)
        {
            this._isDisposed = true;
            if (!disposing)
                return;
            do
                ;
            while (this._blocks.TryDequeue(out MemoryPoolBlock _));
        }
    }
}
