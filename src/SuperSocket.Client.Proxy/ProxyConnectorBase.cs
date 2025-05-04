using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// Provides a base class for proxy connectors.
    /// </summary>
    public abstract class ProxyConnectorBase : ConnectorBase
    {
        private EndPoint _proxyEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyConnectorBase"/> class with the specified proxy endpoint.
        /// </summary>
        /// <param name="proxyEndPoint">The endpoint of the proxy server.</param>
        public ProxyConnectorBase(EndPoint proxyEndPoint)
        {
            _proxyEndPoint = proxyEndPoint;
        }

        /// <summary>
        /// Connects to the specified remote endpoint through the proxy.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous connection operation.</returns>
        protected abstract ValueTask<ConnectState> ConnectProxyAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken);

        /// <summary>
        /// Establishes a connection to the specified remote endpoint through the proxy.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous connection operation. The result contains information about the connection status.</returns>
        /// <remarks>
        /// This method first establishes a connection to the proxy server, and then calls <see cref="ConnectProxyAsync"/> to
        /// establish a connection to the remote endpoint through the proxy.
        /// </remarks>
        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var socketConnector = new SocketConnector() as IConnector;
            var proxyEndPoint = _proxyEndPoint;

            ConnectState result;
            
            try
            {
                result = await socketConnector.ConnectAsync(proxyEndPoint, null, cancellationToken);
                
                if (!result.Result)
                    return result;
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

            return await ConnectProxyAsync(remoteEndPoint, result, cancellationToken);
        }
    }
}