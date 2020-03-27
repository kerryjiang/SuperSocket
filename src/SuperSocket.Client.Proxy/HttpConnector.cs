using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.Http;

namespace SuperSocket.Client.Proxy
{
    public class HttpConnector : ConnectorBase
    {
        private const string _requestTemplate = "CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\nProxy-Connection: Keep-Alive\r\n\r\n";
        private const string _responsePrefix11 = "HTTP/1.1";
        private const string _responsePrefix10 = "HTTP/1.0";
        private const char _space = ' ';
        private EndPoint _proxyEndPoint;

        public HttpConnector(EndPoint proxyEndPoint)
            : base()
        {
            _proxyEndPoint = proxyEndPoint;
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

            var channel = result.CreateChannel<HttpRequest>(new HttpPipelineFilter(), new ChannelOptions());
            
            // send request
            //channel.SendAsync();

            await foreach (var p in channel.RunAsync())
            {
                // validating response
                
            }

            await channel.DetachAsync();
            return result;
        }
    }
}