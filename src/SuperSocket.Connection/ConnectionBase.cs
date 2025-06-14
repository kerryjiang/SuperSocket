using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase.ProxyProtocol;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Provides an abstract base class for connections, defining common connection functionality.
    /// </summary>
    public abstract class ConnectionBase : IConnection
    {
        /// <summary>
        /// Runs the connection asynchronously with the specified pipeline filter.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
        /// <param name="pipelineFilter">The pipeline filter to use for processing data.</param>
        /// <returns>An asynchronous enumerable of package information.</returns>
        public abstract IAsyncEnumerable<TPackageInfo> RunAsync<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter);

        /// <summary>
        /// Sends data over the connection asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends data over the connection asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public abstract ValueTask SendAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a package over the connection asynchronously using the specified encoder and package.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package to send.</typeparam>
        /// <param name="packageEncoder">The encoder to use for the package.</param>
        /// <param name="package">The package to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public abstract ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends data over the connection asynchronously using a custom write action.
        /// </summary>
        /// <param name="write">The action to write data to the pipe writer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public abstract ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a value indicating whether the connection is closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Gets the remote endpoint of the connection.
        /// </summary>
        public EndPoint RemoteEndPoint { get; protected set; }

        /// <summary>
        /// Gets the local endpoint of the connection.
        /// </summary>
        public EndPoint LocalEndPoint { get; protected set; }

        /// <summary>
        /// Gets the reason for the connection closure, if available.
        /// </summary>
        public CloseReason? CloseReason { get; protected set; }

        /// <summary>
        /// Gets the last active time of the connection.
        /// </summary>
        public DateTimeOffset LastActiveTime { get; protected set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets the cancellation token associated with the connection.
        /// </summary>
        public CancellationToken ConnectionToken { get; protected set; }

        /// <summary>
        /// Gets the proxy information associated with the connection.
        /// </summary>
        public ProxyInfo ProxyInfo { get; protected set; }

        /// <summary>
        /// Handles actions to perform when the connection is closed.
        /// </summary>
        protected virtual void OnClosed()
        {
            IsClosed = true;

            var closed = Closed;

            if (closed == null)
                return;

            if (Interlocked.CompareExchange(ref Closed, null, closed) != closed)
                return;

            var closeReason = CloseReason.HasValue ? CloseReason.Value : Connection.CloseReason.Unknown;

            closed.Invoke(this, new CloseEventArgs(closeReason));
        }

        /// <summary>
        /// Occurs when the connection is closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> Closed;

        /// <summary>
        /// Closes the connection asynchronously with the specified reason.
        /// </summary>
        /// <param name="closeReason">The reason for closing the connection.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public abstract ValueTask CloseAsync(CloseReason closeReason);

        /// <summary>
        /// Detaches the connection asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous detach operation.</returns>
        public abstract ValueTask DetachAsync();

        public void Dispose()
        {
            DisposeAsync()
                .AsTask()
                .Wait();
        }

        public ValueTask DisposeAsync()
        {
            return CloseAsync(Connection.CloseReason.LocalClosing);
        }
    }
}
