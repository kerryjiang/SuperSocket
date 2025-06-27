using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a connection that uses a stream for data transmission.
    /// </summary>
    public class StreamPipeConnection : PipeConnection, IStreamConnection
    {
        private Stream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamPipeConnection"/> class with the specified stream, remote endpoint, and connection options.
        /// </summary>
        /// <param name="stream">The stream used for data transmission.</param>
        /// <param name="remoteEndPoint">The remote endpoint of the connection.</param>
        /// <param name="options">The connection options.</param>
        public StreamPipeConnection(Stream stream, EndPoint remoteEndPoint, ConnectionOptions options)
            : this(stream, remoteEndPoint, null, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamPipeConnection"/> class with the specified stream, remote endpoint, local endpoint, and connection options.
        /// </summary>
        /// <param name="stream">The stream used for data transmission.</param>
        /// <param name="remoteEndPoint">The remote endpoint of the connection.</param>
        /// <param name="localEndPoint">The local endpoint of the connection.</param>
        /// <param name="options">The connection options.</param>
        public StreamPipeConnection(Stream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, ConnectionOptions options)
            : base(options)
        {
            _stream = stream;
            RemoteEndPoint = remoteEndPoint;
            LocalEndPoint = localEndPoint;
        }

        /// <summary>
        /// Closes the connection and releases the associated resources.
        /// </summary>
        protected override void Close()
        {
            _stream.Close();
        }

        /// <summary>
        /// Handles actions to perform when the connection is closed.
        /// </summary>
        protected override void OnClosed()
        {
            _stream = null;
            base.OnClosed();
        }

        /// <summary>
        /// Reads data from the input stream into the specified memory buffer.
        /// </summary>
        /// <param name="memory">The memory buffer to fill with data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The number of bytes read from the stream.</returns>
        protected override async ValueTask<int> FillInputPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            return await _stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends data over the stream using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The total number of bytes sent.</returns>
        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var total = 0;

            foreach (var data in buffer)
            {
                await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                total += data.Length;
            }

            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return total;
        }

        /// <summary>
        /// Determines whether the specified exception is ignorable.
        /// </summary>
        /// <param name="e">The exception to check.</param>
        /// <returns><c>true</c> if the exception is ignorable; otherwise, <c>false</c>.</returns>
        protected override bool IsIgnorableException(Exception e)
        {
            if (base.IsIgnorableException(e))
                return true;

            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the stream used for data transmission.
        /// </summary>
        Stream IStreamConnection.Stream
        {
            get { return _stream; }
        }
    }
}