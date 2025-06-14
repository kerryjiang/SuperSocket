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
    /// Represents a connection with methods for sending data, managing state, and handling events.
    /// </summary>
    public interface IConnection : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Runs the connection asynchronously with the specified pipeline filter.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
        /// <param name="pipelineFilter">The pipeline filter to use for processing data.</param>
        /// <returns>An asynchronous enumerable of package information.</returns>
        IAsyncEnumerable<TPackageInfo> RunAsync<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter);

        /// <summary>
        /// Sends data over the connection asynchronously.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends data over the connection asynchronously.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a package over the connection asynchronously using the specified encoder.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package to send.</typeparam>
        /// <param name="packageEncoder">The encoder to use for the package.</param>
        /// <param name="package">The package to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync<TPackage>(IPackageEncoder<TPackage> packageEncoder, TPackage package, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends data over the connection asynchronously using a custom write action.
        /// </summary>
        /// <param name="write">The action to write data to the pipe writer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync(Action<PipeWriter> write, CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the connection asynchronously with the specified reason.
        /// </summary>
        /// <param name="closeReason">The reason for closing the connection.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        ValueTask CloseAsync(CloseReason closeReason);

        /// <summary>
        /// Occurs when the connection is closed.
        /// </summary>
        event EventHandler<CloseEventArgs> Closed;

        /// <summary>
        /// Gets a value indicating whether the connection is closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Gets the remote endpoint of the connection.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the local endpoint of the connection.
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the last active time of the connection.
        /// </summary>
        DateTimeOffset LastActiveTime { get; }

        /// <summary>
        /// Detaches the connection asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous detach operation.</returns>
        ValueTask DetachAsync();

        /// <summary>
        /// Gets the reason for the connection closure, if available.
        /// </summary>
        CloseReason? CloseReason { get; }

        /// <summary>
        /// Gets the cancellation token associated with the connection.
        /// </summary>
        CancellationToken ConnectionToken { get; }

        /// <summary>
        /// Gets the proxy information associated with the connection.
        /// </summary>
        ProxyInfo ProxyInfo { get; }
    }
}
