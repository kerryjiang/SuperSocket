using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    /// <summary>
    /// Provides a base class for connectors that establish connections to remote endpoints.
    /// </summary>
    public abstract class ConnectorBase : IConnector
    {
        /// <summary>
        /// Gets or sets the next connector in the chain.
        /// </summary>
        public IConnector NextConnector { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorBase"/> class.
        /// </summary>
        public ConnectorBase()
        {
        }

        /// <summary>
        /// Asynchronously connects to a remote endpoint.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        protected abstract ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously connects to a remote endpoint and invokes the next connector in the chain if applicable.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        async ValueTask<ConnectState> IConnector.ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var result = await ConnectAsync(remoteEndPoint, state, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return ConnectState.CancelledState;

            var nextConnector = NextConnector;

            if (!result.Result || nextConnector == null)
                return result;            

            return await nextConnector.ConnectAsync(remoteEndPoint, result, cancellationToken);
        }
    }
}