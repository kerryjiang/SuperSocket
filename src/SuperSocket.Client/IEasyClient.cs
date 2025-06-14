using System;
using System.Buffers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client
{
    /// <summary>
    /// Defines methods and properties for managing client connections and data transmission.
    /// </summary>
    public interface IEasyClient : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets or sets the proxy connector for the client.
        /// </summary>
        IConnector Proxy { get; set; }

        /// <summary>
        /// Asynchronously connects to a remote endpoint.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or sets the local endpoint for the client.
        /// </summary>
        IPEndPoint LocalEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the security options for the client.
        /// </summary>
        SecurityOptions Security { get; set; }

        /// <summary>
        /// Starts receiving data from the server.
        /// </summary>
        void StartReceive();

        /// <summary>
        /// Asynchronously sends data to the server.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        /// <summary>
        /// Asynchronously sends data to the server.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlySequence<byte> data);

        /// <summary>
        /// Asynchronously sends a package to the server using the specified encoder.
        /// </summary>
        /// <typeparam name="TSendPackage">The type of the package to send.</typeparam>
        /// <param name="packageEncoder">The encoder to use for the package.</param>
        /// <param name="package">The package to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync<TSendPackage>(IPackageEncoder<TSendPackage> packageEncoder, TSendPackage package);

        /// <summary>
        /// Occurs when the client is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Asynchronously closes the client connection.
        /// </summary>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        ValueTask CloseAsync();
    }

    /// <summary>
    /// Defines methods and properties for managing client connections and data transmission with a specific receive package type.
    /// </summary>
    /// <typeparam name="TReceivePackage">The type of the received package.</typeparam>
    public interface IEasyClient<TReceivePackage> : IEasyClient
        where TReceivePackage : class
    {
        /// <summary>
        /// Asynchronously receives a package from the server.
        /// </summary>
        /// <returns>A task that represents the asynchronous receive operation.</returns>
        ValueTask<TReceivePackage> ReceiveAsync();

        /// <summary>
        /// Occurs when a package is received from the server.
        /// </summary>
        event PackageHandler<TReceivePackage> PackageHandler;
    }

    /// <summary>
    /// Defines methods and properties for managing client connections and data transmission with specific receive and send package types.
    /// </summary>
    /// <typeparam name="TReceivePackage">The type of the received package.</typeparam>
    /// <typeparam name="TSendPackage">The type of the package to send.</typeparam>
    public interface IEasyClient<TReceivePackage, TSendPackage> : IEasyClient<TReceivePackage>
        where TReceivePackage : class
    {
        /// <summary>
        /// Asynchronously sends a package to the server.
        /// </summary>
        /// <param name="package">The package to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        ValueTask SendAsync(TSendPackage package);
    }
}