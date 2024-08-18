using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperSocket.Tests
{
    public class ProxyProtocolV1HostConfigurator : ProxyProtocolHostConfigurator
    {
        protected override byte[] CreateProxyProtocolData(IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint)
        {
            var protocol = InnerHostConfigurator is UdpHostConfigurator ? "UDP" : "TCP";
            var addressVersion = sourceIPEndPoint.Address.AddressFamily == AddressFamily.InterNetwork ? 4 : 6;
            
            var line = $"PROXY {protocol}{addressVersion} {sourceIPEndPoint.Address} {destinationIPEndPoint.Address} {sourceIPEndPoint.Port} {destinationIPEndPoint.Port}\r\n";

            return Encoding.ASCII.GetBytes(line);
        }

        public ProxyProtocolV1HostConfigurator(IHostConfigurator hostConfigurator, IPEndPoint sourceIPEndPoint, IPEndPoint destinationIPEndPoint)
            : base(hostConfigurator, sourceIPEndPoint, destinationIPEndPoint)
        {
        }
    }
}