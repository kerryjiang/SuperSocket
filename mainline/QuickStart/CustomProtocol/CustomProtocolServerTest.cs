using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase;
using System.Net;
using System.Net.Sockets;
using System.IO;
using SuperSocket.Common.Logging;

namespace SuperSocket.QuickStart.CustomProtocol
{
    [TestFixture]
    public class CustomProtocolServerTest
    {

        private CustomProtocolServer m_Server;
        private IServerConfig m_Config;

        private IBootstrap m_Bootstrap;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_Bootstrap = new DefaultBootstrap();

            m_Config = new ServerConfig
                {
                    Port = 911,
                    Ip = "Any",
                    MaxConnectionNumber = 1000,
                    Mode = SocketMode.Tcp,
                    Name = "CustomProtocolServer"
                };

            m_Server = new CustomProtocolServer();

            m_Bootstrap.Initialize(new RootConfig(), new IAppServer[] { m_Server }, new IServerConfig[] { m_Config }, new ConsoleLogFactory());
        }

        [SetUp]
        public void StartServer()
        {
            m_Bootstrap.Start();
        }

        [TearDown]
        public void StopServer()
        {
            m_Bootstrap.Stop();
        }

        [Test]
        public void TestCustomProtocol()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);

                var socketStream = new NetworkStream(socket);
                var reader = new StreamReader(socketStream, Encoding.ASCII, false);
                var writer = new StreamWriter(socketStream, Encoding.ASCII, 1024);

                string charSource = Guid.NewGuid().ToString().Replace("-", string.Empty)
                    + Guid.NewGuid().ToString().Replace("-", string.Empty)
                    + Guid.NewGuid().ToString().Replace("-", string.Empty);

                Random rd = new Random();

                for (int i = 0; i < 10; i++)
                {
                    int startPos = rd.Next(0, charSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, charSource.Length - 1);

                    var currentMessage = charSource.Substring(startPos, endPos - startPos + 1);

                    writer.Write("ECHO {0} {1}", currentMessage.Length.ToString().PadLeft(4, '0'), currentMessage);
                    writer.Flush();

                    var line = reader.ReadLine();
                    Console.WriteLine("Received: " + line);
                    Assert.AreEqual(currentMessage, line);
                }
            }
        }
    }
}
