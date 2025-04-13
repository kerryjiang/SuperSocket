using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;

namespace SuperSocket.WebSocket.Extensions.Compression
{
    /// <summary>
    /// Represents a read-only stream that reads data from a <see cref="ReadOnlySequence{T}"/>.
    /// </summary>
    class ReadOnlySequenceStream : Stream
    {
        private ReadOnlySequence<byte> _sequence;

        /// <summary>
        /// Gets a value indicating whether the stream supports reading. Always returns true.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking. Always returns false.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating whether the stream supports writing. Always returns false.
        /// </summary>
        public override bool CanWrite => false;

        private long _length;

        /// <summary>
        /// Gets the length of the stream.
        /// </summary>
        public override long Length => _length;

        /// <summary>
        /// Gets or sets the position within the stream. Not implemented.
        /// </summary>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlySequenceStream"/> class with the specified sequence.
        /// </summary>
        /// <param name="sequence">The read-only sequence to read data from.</param>
        public ReadOnlySequenceStream(ReadOnlySequence<byte> sequence)
        {
            _sequence = sequence;
            _length = sequence.Length;
        }

        /// <summary>
        /// Flushes the stream. Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="offset">The zero-based byte offset in the buffer at which to begin storing the data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
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

        /// <summary>
        /// Sets the position within the stream. Not supported.
        /// </summary>
        /// <param name="offset">The byte offset relative to the <paramref name="origin"/>.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>Always throws a <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream. Not supported.
        /// </summary>
        /// <param name="value">The desired length of the stream.</param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream. Not supported.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write.</param>
        /// <param name="offset">The zero-based byte offset in the buffer at which to begin copying bytes to the stream.</param>
        /// <param name="count">The number of bytes to write to the stream.</param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
