using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{
    public class HttpConnector : ConnectorBase
    {
        private const string _requestTemplate = "CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\nProxy-Connection: Keep-Alive\r\n";
        private const string _responsePrefix = "HTTP/1.";
        private const char _space = ' ';
        private EndPoint _proxyEndPoint;
        private string _username;
        private string _password;

        public HttpConnector(EndPoint proxyEndPoint)
            : base()
        {
            _proxyEndPoint = proxyEndPoint;
        }

        public HttpConnector(EndPoint proxyEndPoint, string username, string password)
            : this(proxyEndPoint)
        {
            _username = username;
            _password = password;
        }

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

            var encoding = Encoding.ASCII;
            var request = string.Empty;
            var channel = result.CreateChannel<TextPackageInfo>(new LinePipelineFilter(encoding), new ChannelOptions());

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
            await channel.SendAsync((writer) =>
            {
                writer.Write(request, encoding);
                writer.Write("Proxy-Authorization: Basic ", encoding);
                writer.Write(Convert.ToBase64String(encoding.GetBytes($"{_username}:{_password}")), encoding);
                writer.Write("\r\n\r\n", encoding);
            });
            
            await foreach (var p in channel.RunAsync())
            {
                var pos = p.Text.IndexOf(_space);

                // validating response
                if (!p.Text.StartsWith(_responsePrefix, StringComparison.OrdinalIgnoreCase) || pos <= 0)
                {
                    await channel.CloseAsync();

                    return new ConnectState
                    {
                        Result = false,
                        Exception = new Exception("Invalid response")
                    };
                }

                if (!int.TryParse(p.Text.AsSpan().Slice(pos + 1), out var statusCode))
                {
                    await channel.CloseAsync();

                    return new ConnectState
                    {
                        Result = false,
                        Exception = new Exception("Invalid response")
                    };
                }

                if (statusCode < 200 || statusCode > 299)
                {
                    await channel.CloseAsync();

                    return new ConnectState
                    {
                        Result = false,
                        Exception = new Exception($"Invalid status code {statusCode}")
                    };
                }

                break;
            }

            await channel.DetachAsync();
            return result;
        }
    }
}