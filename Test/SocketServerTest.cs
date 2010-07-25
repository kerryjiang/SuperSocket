using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.SocketServiceCore;
using SuperSocket.Common;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace SuperSocket.Test
{
    [TestFixture]
    public class SocketServerTest
    {
        MockRepository mocks = new MockRepository();
        TestServer m_Server;
        int m_Port = 100;
        string m_ServerName = "My Test Server";

        [SetUp]
        public void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());
            IServerConfig config = mocks.DynamicMock<IServerConfig>();

            using (mocks.Record())
            {
                config.Expect(c => c.Name).Return("My Test Server").Repeat.Any();
                config.Expect(c => config.Ip).Return("Any").Repeat.Any();
                config.Expect(c => config.MaxConnectionNumber).Return(10).Repeat.Any();
                config.Expect(c => config.Port).Return(100).Repeat.Any();
                config.Expect(c => config.Mode).Return(SocketMode.Async).Repeat.Any();
            }

            m_Server = new TestServer();
            m_Server.Setup(string.Empty, config, string.Empty);
        }

        [Test]
        public void TestStartStop()
        {
            StartServer();
            StopServer();
        }

        private void StartServer()
        {
            m_Server.Start();
            Console.WriteLine("Socket server has been started!");
        }

        private void StopServer()
        {
            m_Server.Stop();
            Console.WriteLine("Socket server has been stopped!");
        }

        [Test]
        public void TestWelcomeMessage()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);
            Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(serverAddress);
            Stream socketStream = new NetworkStream(socket);
            using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
            using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
            {
                string welcomeString = reader.ReadLine();
                Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, m_ServerName), welcomeString);
            }

            StopServer();
        }
    }
}
