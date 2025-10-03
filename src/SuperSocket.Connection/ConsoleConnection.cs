using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a connection that uses the console's standard input/output streams for communication.
    /// </summary>
    public class ConsoleConnection : StreamPipeConnection
    {
        private static readonly EndPoint _consoleEndPoint = new ConsoleEndPoint();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleConnection"/> class.
        /// </summary>
        /// <param name="options">The connection options.</param>
        public ConsoleConnection(ConnectionOptions options)
            : base(CreateConsoleStream(), _consoleEndPoint, _consoleEndPoint, options)
        {
        }

        /// <summary>
        /// Creates a bidirectional stream that combines console input and output.
        /// </summary>
        /// <returns>A stream that reads from stdin and writes to stdout.</returns>
        private static Stream CreateConsoleStream()
        {
            return new ConsoleStream();
        }

        /// <summary>
        /// Represents an endpoint for console communication.
        /// </summary>
        private class ConsoleEndPoint : EndPoint
        {
            public override string ToString() => "Console";
        }

        /// <summary>
        /// A stream implementation that combines console input and output operations.
        /// </summary>
        private class ConsoleStream : Stream
        {
            private readonly Stream _inputStream;
            private readonly Stream _outputStream;

            public ConsoleStream()
            {
                _inputStream = Console.OpenStandardInput();
                _outputStream = Console.OpenStandardOutput();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position 
            { 
                get => throw new NotSupportedException(); 
                set => throw new NotSupportedException(); 
            }

            public override void Flush()
            {
                _outputStream.Flush();
            }

            public override async Task FlushAsync(CancellationToken cancellationToken)
            {
                await _outputStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _inputStream.Read(buffer, offset, count);
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return await _inputStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return await _inputStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
                _outputStream.Write(buffer, offset, count);
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await _outputStream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }

            public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                await _outputStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Don't dispose the console streams as they are managed by the runtime
                    // _inputStream?.Dispose();
                    // _outputStream?.Dispose();
                }
                base.Dispose(disposing);
            }

            public override async ValueTask DisposeAsync()
            {
                // Don't dispose the console streams as they are managed by the runtime
                // if (_inputStream != null)
                //     await _inputStream.DisposeAsync().ConfigureAwait(false);
                // if (_outputStream != null)
                //     await _outputStream.DisposeAsync().ConfigureAwait(false);
                await base.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}