using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Extensions.Compression
{
    /// <summary>
    /// Represents a writable stream that writes data into a sequence.
    /// </summary>
    class WritableSequenceStream : Stream
    {
        /// <summary>
        /// Gets a value indicating whether the stream supports reading. Always returns false.
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking. Always returns false.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating whether the stream supports writing. Always returns true.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets the length of the stream. Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override long Length { get => throw new NotSupportedException(); }

        /// <summary>
        /// Gets or sets the position within the stream. Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        private SequenceSegment _head;

        private SequenceSegment _tail;

        private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

        /// <summary>
        /// Flushes the stream. Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads data from the stream. Not supported.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="offset">The byte offset in the buffer at which to begin storing data.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>Always throws a <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
        /// Writes a sequence of bytes to the current stream.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write.</param>
        /// <param name="offset">The zero-based byte offset in the buffer at which to begin copying bytes to the stream.</param>
        /// <param name="count">The number of bytes to write to the stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var data = _arrayPool.Rent(count);

            Array.Copy(buffer, offset, data, 0, count);

            var segment = new SequenceSegment(data, count);

            if (_head == null)
                _tail = _head = segment;
            else
                _tail.SetNext(segment);
        }

        /// <summary>
        /// Gets the underlying sequence of bytes written to the stream.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySequence{T}"/> representing the data written to the stream.</returns>
        public ReadOnlySequence<byte> GetUnderlyingSequence()
        {
            return new ReadOnlySequence<byte>(_head, 0, _tail, _tail.Memory.Length);
        }
    }
}
