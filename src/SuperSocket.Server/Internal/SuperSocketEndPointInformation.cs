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
            IPEndPoint = new IPEndPoint(IPAddress.Parse(listenOptions.Ip), listenOptions.Port);
        }

        public ListenType Type { get; set; }

        public IPEndPoint IPEndPoint { get; set; }

        public string SocketPath { get; set; }

        public ulong FileHandle { get; set; }

        public FileHandleType HandleType { get; set; }

        public bool NoDelay { get; set; }
    }
}