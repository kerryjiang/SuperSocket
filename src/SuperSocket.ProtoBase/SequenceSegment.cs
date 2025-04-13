using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Represents a segment of a sequence of bytes.
    /// </summary>
    public class SequenceSegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// The pooled buffer used by the segment.
        /// </summary>
        private byte[] _pooledBuffer;

        /// <summary>
        /// Indicates whether the buffer is pooled.
        /// </summary>
        private bool _pooled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceSegment"/> class with the specified buffer and length.
        /// </summary>
        /// <param name="pooledBuffer">The buffer used by the segment.</param>
        /// <param name="length">The length of the segment.</param>
        /// <param name="pooled">Indicates whether the buffer is pooled.</param>
        public SequenceSegment(byte[] pooledBuffer, int length, bool pooled)
        {
            _pooledBuffer = pooledBuffer;
            _pooled = pooled;
            this.Memory = new ArraySegment<byte>(pooledBuffer, 0, length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceSegment"/> class with the specified buffer and length.
        /// </summary>
        /// <param name="pooledBuffer">The buffer used by the segment.</param>
        /// <param name="length">The length of the segment.</param>
        public SequenceSegment(byte[] pooledBuffer, int length)
            : this(pooledBuffer, length, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceSegment"/> class with the specified memory.
        /// </summary>
        /// <param name="memory">The memory used by the segment.</param>
        public SequenceSegment(ReadOnlyMemory<byte> memory)
        {
            this.Memory = memory;
        }

        /// <summary>
        /// Sets the next segment in the sequence.
        /// </summary>
        /// <param name="segment">The next segment.</param>
        /// <returns>The next segment.</returns>
        public SequenceSegment SetNext(SequenceSegment segment)
        {
            segment.RunningIndex = RunningIndex + Memory.Length;
            Next = segment;
            return segment;
        }

        /// <summary>
        /// Creates a new segment by copying data from the specified memory.
        /// </summary>
        /// <param name="memory">The memory to copy data from.</param>
        /// <returns>The created segment.</returns>
        public static SequenceSegment CopyFrom(ReadOnlyMemory<byte> memory)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(memory.Length);
            memory.Span.CopyTo(new Span<byte>(buffer));
            return new SequenceSegment(buffer, memory.Length);
        }

        /// <summary>
        /// Releases the resources used by the segment.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_pooled && _pooledBuffer != null)
                        ArrayPool<byte>.Shared.Return(_pooledBuffer);
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources used by the segment.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
