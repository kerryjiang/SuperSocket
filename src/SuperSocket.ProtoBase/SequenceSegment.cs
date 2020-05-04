using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public class SequenceSegment : ReadOnlySequenceSegment<byte>, IDisposable
    {
        private bool disposedValue;

        private byte[] _pooledBuffer;

        private SequenceSegment(byte[] pooledBuffer, int length)
        {
            _pooledBuffer = pooledBuffer;
            this.Memory = new ArraySegment<byte>(pooledBuffer, 0, length);
        }

        public SequenceSegment(ReadOnlyMemory<byte> memory)
        {
            this.Memory = memory;
        }

        public SequenceSegment SetNext(SequenceSegment segment)
        {
            segment.RunningIndex = RunningIndex + Memory.Length;
            Next = segment;
            return segment;
        }

        public static SequenceSegment CopyFrom(ReadOnlyMemory<byte> memory)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(memory.Length);
            memory.Span.CopyTo(new Span<byte>(buffer));
            return new SequenceSegment(buffer, memory.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_pooledBuffer != null)
                        ArrayPool<byte>.Shared.Return(_pooledBuffer);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
