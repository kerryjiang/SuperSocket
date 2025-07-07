using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a UDP-based pipe connection.
    /// </summary>
    public class UdpPipeConnection : VirtualConnection, IConnectionWithSessionIdentifier
    {
        private Socket _socket;

        private bool _enableSendingPipe;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPipeConnection"/> class with the specified socket, connection options, and remote endpoint.
        /// </summary>
        /// <param name="socket">The socket used for the connection.</param>
        /// <param name="options">The connection options.</param>
        /// <param name="remoteEndPoint">The remote endpoint of the connection.</param>
        public UdpPipeConnection(Socket socket, ConnectionOptions options, IPEndPoint remoteEndPoint)
            : this(socket, options, remoteEndPoint, $"{remoteEndPoint.Address}:{remoteEndPoint.Port}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPipeConnection"/> class with the specified socket, connection options, remote endpoint, and session identifier.
        /// </summary>
        /// <param name="socket">The socket used for the connection.</param>
        /// <param name="options">The connection options.</param>
        /// <param name="remoteEndPoint">The remote endpoint of the connection.</param>
        /// <param name="sessionIdentifier">The session identifier for the connection.</param>
        public UdpPipeConnection(Socket socket, ConnectionOptions options, IPEndPoint remoteEndPoint, string sessionIdentifier)
            : base(options)
        {
            _socket = socket;
            _enableSendingPipe = "true".Equals(options.Values?["enableSendingPipe"], StringComparison.OrdinalIgnoreCase);
            RemoteEndPoint = remoteEndPoint;
            SessionIdentifier = sessionIdentifier;
        }

        /// <summary>
        /// Gets the session identifier for the connection.
        /// </summary>
        public string SessionIdentifier { get; }

        /// <summary>
        /// Closes the connection and completes the input writer.
        /// </summary>
        protected override void Close()
        {
            Input.Writer.Complete();
        }

        /// <summary>
        /// Throws a <see cref="NotSupportedException"/> as filling the pipe with data is not supported for UDP connections.
        /// </summary>
        /// <param name="memory">The memory buffer to fill with data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override ValueTask<int> FillInputPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sends data over the UDP connection using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The total number of bytes sent.</returns>
        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (_enableSendingPipe || buffer.IsSingleSegment)
            {
                var total = 0;

                foreach (var piece in buffer)
                {
                    total += await _socket
                        .SendToAsync(GetArrayByMemory(piece), SocketFlags.None, RemoteEndPoint)
                        .ConfigureAwait(false);
                }

                return total;
            }

            var pool = ArrayPool<byte>.Shared;
            var destBuffer = pool.Rent((int)buffer.Length);

            try
            {
                MergeBuffer(ref buffer, destBuffer);

                return await _socket
                    .SendToAsync(new ArraySegment<byte>(destBuffer, 0, (int)buffer.Length), SocketFlags.None, RemoteEndPoint)
                    .ConfigureAwait(false);
            }
            finally
            {
                pool.Return(destBuffer);
            }
        }

        /// <summary>
        /// Processes send operations for the connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task ProcessSends()
        {
            if (_enableSendingPipe)
                return base.ProcessSends();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Merges the specified buffer into the destination buffer.
        /// </summary>
        /// <param name="buffer">The source buffer to merge.</param>
        /// <param name="destBuffer">The destination buffer to fill.</param>
        private void MergeBuffer(ref ReadOnlySequence<byte> buffer, byte[] destBuffer)
        {
            Span<byte> destSpan = destBuffer;

            var total = 0;

            foreach (var piece in buffer)
            {
                piece.Span.CopyTo(destSpan);
                total += piece.Length;
                destSpan = destSpan.Slice(piece.Length);
            }
        }

        /// <summary>
        /// Sends data asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (_enableSendingPipe)
            {
                await base
                    .SendAsync(buffer, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            await SendOverIOAsync(new ReadOnlySequence<byte>(buffer), cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends data asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async ValueTask SendAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            if (_enableSendingPipe)
            {
                await base
                    .SendAsync(buffer, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            await SendOverIOAsync(buffer, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a package asynchronously using the specified encoder and package.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package.</typeparam>
        /// <param name="packageEncoder">The encoder used to encode the package.</param>
        /// <param name="package">The package to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken)
        {
            if (_enableSendingPipe)
            {
                await base
                    .SendAsync(packageEncoder, package, cancellationToken)
                    .ConfigureAwait(false);

                return;
            }

            try
            {
                await SendLock
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);

                var writer = OutputWriter;

                WritePackageWithEncoder<TPackage>(writer, packageEncoder, package);

                await writer
                    .FlushAsync(cancellationToken)
                    .ConfigureAwait(false);

                await ProcessOutputRead(Output.Reader)
                    .ConfigureAwait(false);
            }
            finally
            {
                SendLock.Release();
            }
        }

        /// <summary>
        /// Sends data asynchronously using the specified write action.
        /// </summary>
        /// <param name="write">The action to write data to the pipe writer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken)
        {
            if (_enableSendingPipe)
            {
                await base
                    .SendAsync(write, cancellationToken)
                    .ConfigureAwait(false);

                return;
            }

            throw new NotSupportedException($"The method SendAsync(Action<PipeWriter> write) cannot be used when noSendingPipe is true.");
        }
    }
}