using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using System.Net;

namespace SuperSocket.Server
{
    internal class SuperSocketEndPointInformation : IEndPointInformation
    {
        public SuperSocketEndPointInformation(ListenOptions listenOptions)
        {
            Type = ListenType.IPEndPoint;

            var ip = IPAddress.None;

            if ("any".Equals(listenOptions.Ip, StringComparison.OrdinalIgnoreCase))
            {
                ip = IPAddress.Any;
            }
            else if ("ipv6any".Equals(listenOptions.Ip, StringComparison.OrdinalIgnoreCase))
            {
                ip = IPAddress.IPv6Any;
            }
            else
            {
                ip = IPAddress.Parse(listenOptions.Ip);
            }

            IPEndPoint = new IPEndPoint(ip, listenOptions.Port);
        }

        public ListenType Type { get; set; }

        public IPEndPoint IPEndPoint { get; set; }

        public string SocketPath { get; set; }

        public ulong FileHandle { get; set; }

        public FileHandleType HandleType { get; set; }

        public bool NoDelay { get; set; }
    }
}