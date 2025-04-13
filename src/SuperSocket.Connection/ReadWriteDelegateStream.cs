using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a stream that delegates read and write operations to separate streams.
    /// </summary>
    public class ReadWriteDelegateStream : Stream
    {
        /// <summary>
        /// Gets the base stream associated with this delegate stream.
        /// </summary>
        public Stream BaseStream { get; }

        private Stream readStream;
        private Stream writeStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteDelegateStream"/> class with the specified base, read, and write streams.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        /// <param name="readStream">The stream used for reading operations.</param>
        /// <param name="writeStream">The stream used for writing operations.</param>
        public ReadWriteDelegateStream(Stream stream, Stream readStream, Stream writeStream)
        {
            this.readStream = readStream;
            this.writeStream = writeStream;
            this.BaseStream = stream;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <inheritdoc/>
        public override void Flush()
        {
            writeStream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return readStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            writeStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            writeStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            return readStream.Read(buffer);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return readStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return writeStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            BaseStream.Close();
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            readStream.CopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return readStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            await readStream.DisposeAsync();
            await writeStream.DisposeAsync();
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return readStream.EndRead(asyncResult);
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            writeStream.EndWrite(asyncResult);
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return writeStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return readStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return readStream.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            return readStream.ReadByte();
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return writeStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return writeStream.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            writeStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override bool CanTimeout => BaseStream.CanTimeout;

        /// <inheritdoc/>
        public override int ReadTimeout { get => BaseStream.ReadTimeout; set => BaseStream.ReadTimeout = value; }

        /// <inheritdoc/>
        public override int WriteTimeout { get => BaseStream.WriteTimeout; set => BaseStream.WriteTimeout = value; }
    }
}