using System.Net;
using SuperSocket.Config;

namespace SuperSocket
{
    public class Listener
    {
        public Listener()
        {

        }

        public Listener(ListenerConfig config)
        {
            var ipAddress = IPAddress.None;

            switch(config.Ip.ToLower())
            {
                case("any"):
                    ipAddress = IPAddress.Any;
                    break;

                case("ipv6any"):
                    ipAddress = IPAddress.IPv6Any;
                    break;

                default:
                    ipAddress = IPAddress.Parse(config.Ip);
                    break;
            }
            
            EndPoint = new IPEndPoint(ipAddress, config.Port);
        }

        public IPEndPoint EndPoint { get; set; }
    }
}