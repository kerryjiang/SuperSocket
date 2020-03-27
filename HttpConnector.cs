using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.Client;

namespace SuperSocket.Client.Proxy
{
    public class HttpConnector : ProxyConnectorBase<ProxyDataPackageInfo>
    {
        private string _userNameAuthenRequest;

        private const string _responsePrefix = "HTTP/1.";

        private const char _space = ' ';

        private const string _requestTemplate = "CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\nProxy-Connection: Keep-Alive\r\n\r\n";

        private const string _requestAuthenTemplate = "CONNECT {0}:{1} HTTP/1.1\r\nHost: {0}:{1}\r\nProxy-Authorization: Basic {2}\r\nProxy-Connection: Keep-Alive\r\n\r\n";

        public HttpConnector(EndPoint proxyEndPoint) : this(proxyEndPoint, null)
        {
        }

        public HttpConnector(EndPoint proxyEndPoint, string targetHostName) : base(proxyEndPoint, targetHostName)
        {
        }

        public HttpConnector(EndPoint proxyEndPoint, string userName, string passWord, string targetHostName) : this(proxyEndPoint, targetHostName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("账号不能为空");

            if (string.IsNullOrEmpty(passWord))
                throw new ArgumentNullException("密码不能为空");

            _userNameAuthenRequest = Convert.ToBase64String(ASCIIEncoding.GetBytes(string.Join(":", userName, passWord)));
        }

        protected override IChannel<ProxyDataPackageInfo> GetPipeChannel(ConnectState state)
        {
            return state.CreateChannel(new HttpPipeLineFilter(), new ChannelOptions());
        }

        protected async override ValueTask<bool> ProcessConnect(IChannel channel)
        {
            var handshakeBuffer = GetEndPointBytes();

            await channel.SendAsync(handshakeBuffer);

            var responsePackageInfo = await base.ReceiveAsync();

            if (responsePackageInfo == null)
                throw new Exception("protocol error: invalid response");

            var lineReader = new StringReader(ASCIIEncoding.GetString(responsePackageInfo.Data));

            var line = lineReader.ReadLine();

            if (string.IsNullOrEmpty(line))
                throw new Exception("protocol error: invalid response");

            //HTTP/1.1 2** OK
            var pos = line.IndexOf(_space);

            if (pos <= 0 || line.Length <= (pos + 2))
                throw new Exception("protocol error: invalid response");

            var httpProtocol = line.Substring(0, pos);

            if (!httpProtocol.Contains(_responsePrefix))
                throw new Exception("protocol error: invalid protocol");

            var statusPos = line.IndexOf(_space, pos + 1);

            if (statusPos < 0)
                throw new Exception("protocol error: invalid response");

            //Status code should be 2**
            if (!int.TryParse(line.Substring(pos + 1, statusPos - pos - 1), out int statusCode) || (statusCode > 299 || statusCode < 200))
                throw new Exception("the proxy server refused the connection");

            return true;
        }

        private byte[] GetEndPointBytes()
        {
            string request;

            if (base.TargetEndPoint is DnsEndPoint)
            {
                var targetDnsEndPoint = (DnsEndPoint)base.TargetEndPoint;

                if (string.IsNullOrEmpty(_userNameAuthenRequest))
                    request = string.Format(_requestTemplate, targetDnsEndPoint.Host, targetDnsEndPoint.Port);
                else
                    request = string.Format(_requestAuthenTemplate, targetDnsEndPoint.Host, targetDnsEndPoint.Port, _userNameAuthenRequest);
            }
            else
            {
                var targetIPEndPoint = (IPEndPoint)base.TargetEndPoint;

                if (string.IsNullOrEmpty(_userNameAuthenRequest))
                    request = string.Format(_requestTemplate, targetIPEndPoint.Address, targetIPEndPoint.Port);
                else
                    request = string.Format(_requestAuthenTemplate, targetIPEndPoint.Address, targetIPEndPoint.Port, _userNameAuthenRequest);
            }

            return ASCIIEncoding.GetBytes(request);
        }
    }
}