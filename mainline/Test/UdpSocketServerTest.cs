using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.SocketServiceCore;
using NUnit.Framework;
using System.Net.Sockets;

namespace SuperSocket.Test
{
    [TestFixture]
    public class UdpSocketServerTest : SocketServerTest
    {
        protected override IServerConfig DefaultServerConfig
        {
            get
            {
                return new ServerConfig
                    {
                        Ip = "127.0.0.1",
                        LogCommand = true,
                        MaxConnectionNumber = 3,
                        Mode = SocketMode.Udp,
                        Name = "Udp Test Socket Server",
                        Port = 2196
                    };
            }
        }

        protected override System.Net.Sockets.Socket CreateClientSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
    }
}
