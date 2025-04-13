using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    /// <summary>
    /// Defines methods and properties for connecting to a remote endpoint.
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        /// Asynchronously connects to a remote endpoint.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state object (optional).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next connector in the chain.
        /// </summary>
        IConnector NextConnector { get; }
    }
}