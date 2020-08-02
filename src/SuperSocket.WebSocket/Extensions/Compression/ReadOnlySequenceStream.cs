using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

namespace SuperSocket.WebSocket.Extensions.Compression
{
    class ReadOnlySequenceStream : Stream
    {
        private ReadOnlySequence<byte> _sequence;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        private long _length;

        public override long Length => _length;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ReadOnlySequenceStream(ReadOnlySequence<byte> sequence)
        {
            _sequence = sequence;
            _length = sequence.Length;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var firstSpan = _sequence.FirstSpan;
            
            if (firstSpan.IsEmpty)
                return 0;

            var len = Math.Min(firstSpan.Length, count);
            var destSpan = new Span<byte>(buffer, offset, len);

            firstSpan.CopyTo(destSpan);
            _sequence = _sequence.Slice(len);
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
