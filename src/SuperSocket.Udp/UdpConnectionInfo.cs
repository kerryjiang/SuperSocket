using System;
using System.Net;
using System.Net.Sockets;
using SuperSocket.Connection;

namespace SuperSocket.Udp
{
    internal struct UdpConnectionInfo
    {
        public Socket Socket { get; set; }

        public ConnectionOptions ConnectionOptions{ get; set; }

        public string SessionIdentifier { get; set; }

        public IPEndPoint RemoteEndPoint { get; set; }
    }
}