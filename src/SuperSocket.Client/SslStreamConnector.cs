using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    /// <summary>
    /// Represents a connector that establishes SSL/TLS connections using <see cref="SslStream"/>.
    /// </summary>
    public class SslStreamConnector : ConnectorBase
    {
        /// <summary>
        /// Gets the SSL/TLS client authentication options.
        /// </summary>
        public SslClientAuthenticationOptions Options { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SslStreamConnector"/> class with the specified authentication options.
        /// </summary>
        /// <param name="options">The SSL/TLS client authentication options.</param>
        public SslStreamConnector(SslClientAuthenticationOptions options)
            : base()
        {
            Options = options;
        }

        /// <summary>
        /// Asynchronously connects to a remote endpoint using SSL/TLS.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var targetHost = Options.TargetHost;

            if (string.IsNullOrEmpty(targetHost))
            {
                if (remoteEndPoint is DnsEndPoint remoteDnsEndPoint)
                    targetHost = remoteDnsEndPoint.Host;
                else if (remoteEndPoint is IPEndPoint remoteIPEndPoint)
                    targetHost = remoteIPEndPoint.Address.ToString();

                Options.TargetHost = targetHost;
            }

            var socket = state.Socket;

            if (socket == null)
                throw new Exception("Socket from previous connector is null.");
            
            try
            {
                var stream = new SslStream(new NetworkStream(socket, true), false);
                await stream.AuthenticateAsClientAsync(Options, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return ConnectState.CancelledState;

                state.Stream = stream;
                return state;
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }
        }
    }
}