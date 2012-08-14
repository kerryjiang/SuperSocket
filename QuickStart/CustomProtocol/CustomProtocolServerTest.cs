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

namespace SuperSocket.QuickStart.CustomProtocol
{
    [TestFixture]
    public class CustomProtocolServerTest
    {

        private CustomProtocolServer m_Server;
        private IServerConfig m_Config;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_Config = new ServerConfig
                {
                    Port = 911,
                    Ip = "Any",
                    MaxConnectionNumber = 1000,
                    Mode = SocketMode.Tcp,
                    Name = "CustomProtocolServer"
                };

            m_Server = new CustomProtocolServer();
            m_Server.Setup(m_Config, logFactory: new ConsoleLogFactory());
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

        [Test]
        public void TestCustomProtocol()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);

                var socketStream = new NetworkStream(socket);
                var reader = new StreamReader(socketStream, Encoding.ASCII, false);

                string charSource = Guid.NewGuid().ToString().Replace("-", string.Empty)
                    + Guid.NewGuid().ToString().Replace("-", string.Empty)
                    + Guid.NewGuid().ToString().Replace("-", string.Empty);

                Random rd = new Random();

                for (int i = 0; i < 10; i++)
                {
                    int startPos = rd.Next(0, charSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, charSource.Length - 1);

                    var currentMessage = charSource.Substring(startPos, endPos - startPos + 1);

                    byte[] requestNameData = Encoding.ASCII.GetBytes("ECHO");
                    socketStream.Write(requestNameData, 0, requestNameData.Length);
                    var data = Encoding.ASCII.GetBytes(currentMessage);
                    socketStream.Write(new byte[] { (byte)(data.Length / 256), (byte)(data.Length % 256) }, 0, 2);
                    socketStream.Write(data, 0, data.Length);
                    socketStream.Flush();

                    Console.WriteLine("Sent: " + currentMessage);

                    var line = reader.ReadLine();
                    Console.WriteLine("Received: " + line);
                    Assert.AreEqual(currentMessage, line);
                }
            }
        }
    }
}
