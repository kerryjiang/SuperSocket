using System;
using System.Net;
using System.Security.Authentication;

namespace SuperSocket.Server.Abstractions
{
    public class ListenOptions
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string Path { get; set; }

        public int BackLog { get; set; }

        public bool NoDelay { get; set; }

        public SslProtocols Security { get; set; }

        public CertificateOptions CertificateOptions { get; set; }
                
        public IPEndPoint ToEndPoint()
        {
            var ip = this.Ip;
            var port = this.Port;

            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.Any;
            }
            else if ("IpV6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.IPv6Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            return new IPEndPoint(ipAddress, port);
        }

        public override string ToString()
        {
            return $"{nameof(Ip)}={Ip}, {nameof(Port)}={Port}, {nameof(Security)}={Security}, {nameof(Path)}={Path}, {nameof(BackLog)}={BackLog}, {nameof(NoDelay)}={NoDelay}";
        }
    }
}