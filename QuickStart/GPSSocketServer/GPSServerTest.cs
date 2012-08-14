using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    [TestFixture]
    public class GPSServerTest
    {
        private GPSServer m_Server;
        private IServerConfig m_Config;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_Config = new ServerConfig
            {
                Port = 555,
                Ip = "Any",
                MaxConnectionNumber = 10,
                Mode = SocketMode.Tcp,
                Name = "GPSServer"
            };

            m_Server = new GPSServer();
            m_Server.Setup(new RootConfig(), m_Config, logFactory: new ConsoleLogFactory());
        }

        [SetUp]
        public void StartServer()
        {
            m_Server.Start();
        }

        [TearDown]
        public void StopServer()
        {
            m_Server.Stop();
        }

        private static byte[] m_StartMark = new byte[] { 0x68, 0x68 };
        private static byte[] m_EndMark = new byte[] { 0x0d, 0x0a };

        [Test]
        public void TestCustomProtocol()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            Random rd = new Random();

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);

                for (int i = 0; i < 10; i++)
                {
                    List<byte> dataSource = new List<byte>();

                    int messageRepeat = rd.Next(1, 5);

                    StringBuilder sb = new StringBuilder();

                    for (int j = 0; j < messageRepeat; j++)
                    {
                        sb.Append(Guid.NewGuid().ToString().Replace("-", string.Empty));
                    }

                    dataSource.AddRange(m_StartMark);
                    dataSource.AddRange(Encoding.ASCII.GetBytes(sb.ToString(0, rd.Next(20, sb.Length))));
                    dataSource.AddRange(m_EndMark);

                    byte[] data = dataSource.ToArray();

                    if(i % 2 == 0)
                        data[15] = 0x10;
                    else
                        data[15] = 0x1a;

                    socket.Send(data);

                    byte[] response = new byte[5];

                    int read = 0;

                    while (read < response.Length)
                    {
                        read += socket.Receive(response, read, response.Length - read, SocketFlags.None);
                    }

                    Assert.AreEqual(0x54, response[0]);
                    Assert.AreEqual(0x68, response[1]);
                    Assert.AreEqual(0x1a, response[2]);
                    Assert.AreEqual(0x0d, response[3]);
                    Assert.AreEqual(0x0a, response[4]);
                }
            }
        }
    }
}
