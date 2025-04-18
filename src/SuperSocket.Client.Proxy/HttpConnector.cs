using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// Represents a connector for HTTP proxy connections.
    /// </summary>
    public class HttpConnector : ProxyConnectorBase
    {
        private const string _requestTemplate = "CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\nProxy-Connection: Keep-Alive\r\n";
        private const string _responsePrefix = "HTTP/1.";
        private const char _space = ' ';
        private string _username;
        private string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConnector"/> class with the specified proxy endpoint.
        /// </summary>
        /// <param name="proxyEndPoint">The endpoint of the HTTP proxy server.</param>
        public HttpConnector(EndPoint proxyEndPoint)
            : base(proxyEndPoint)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConnector"/> class with the specified proxy endpoint, username, and password.
        /// </summary>
        /// <param name="proxyEndPoint">The endpoint of the HTTP proxy server.</param>
        /// <param name="username">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        public HttpConnector(EndPoint proxyEndPoint, string username, string password)
            : this(proxyEndPoint)
        {
            _username = username;
            _password = password;
        }

        protected override async ValueTask<ConnectState> ConnectProxyAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var encoding = Encoding.ASCII;
            var request = string.Empty;
            var connection = state.CreateConnection(new ConnectionOptions { ReadAsDemand = true });

            var packStream = connection.GetPackageStream(new LinePipelineFilter(encoding));

            if (remoteEndPoint is DnsEndPoint dnsEndPoint)
            {
                request = string.Format(_requestTemplate, dnsEndPoint.Host, dnsEndPoint.Port);
            }
            else if (remoteEndPoint is IPEndPoint ipEndPoint)
            {
                request = string.Format(_requestTemplate, ipEndPoint.Address, ipEndPoint.Port);
            }
            else
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = new Exception($"The endpint type {remoteEndPoint.GetType().ToString()} is not supported.")
                };
            }

            // send request
            await connection.SendAsync((writer) =>
            {
                writer.Write(request, encoding);

                if (!string.IsNullOrEmpty(_username) || !string.IsNullOrEmpty(_password))
                {
                    writer.Write("Proxy-Authorization: Basic ", encoding);
                    writer.Write(Convert.ToBase64String(encoding.GetBytes($"{_username}:{_password}")), encoding);
                    writer.Write("\r\n\r\n", encoding);
                }
                else
                {
                    writer.Write("\r\n", encoding);
                }
            });
            
            var p = await packStream.ReceiveAsync();

            if (!HandleResponse(p, out string errorMessage))
            {
                await connection.CloseAsync(CloseReason.ProtocolError);

                return new ConnectState
                {
                    Result = false,
                    Exception = new Exception(errorMessage)
                };
            }

            await connection.DetachAsync();
            return state;
        }

        private bool HandleResponse(TextPackageInfo p, out string message)
        {
            message = string.Empty;

            if (p == null)
                return false;

            var pos = p.Text.IndexOf(_space);

            // validating response
            if (!p.Text.StartsWith(_responsePrefix, StringComparison.OrdinalIgnoreCase) || pos <= 0)
            {
                message = "Invalid response";
                return false;
            }

            if (!int.TryParse(p.Text.AsSpan().Slice(pos + 1, 3), out var statusCode))
            {
                message = "Invalid response";
                return false;
            }

            if (statusCode < 200 || statusCode > 299)
            {
                message = $"Invalid status code {statusCode}";
                return false;
            }

            return true;
        }
    }
}