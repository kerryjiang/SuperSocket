using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.SocketServiceCore;
using NUnit.Framework;
using System.Net.Sockets;
using SuperSocket.Common;
using System.Net;

namespace SuperSocket.Test
{
    [TestFixture]
    public class UdpSocketServerTest
    {
        private TestServer m_Server;
        private IServerConfig m_Config;

        protected IServerConfig DefaultServerConfig
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

        public UdpSocketServerTest()
        {
            m_Config = DefaultServerConfig;
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            m_Server = new TestServer();
            m_Server.Setup(string.Empty, m_Config, string.Empty);
        }

        [TestFixtureTearDown]
        public void Stop()
        {
            StopServer();
        }

        protected Socket CreateClientSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private void StartServer()
        {
            if (m_Server.IsRunning)
                return;

            m_Server.Start();
            Console.WriteLine("The UDP Socket server is started!");
        }

        private void StopServer()
        {
            if (!m_Server.IsRunning)
                return;

            m_Server.Stop();
            Console.WriteLine("The UDP Socket server is stopped!");
        }

        [Test, Repeat(10)]
        public void TestStartStop()
        {
            StartServer();
            Assert.IsTrue(m_Server.IsRunning);
            StopServer();
            Assert.IsFalse(m_Server.IsRunning);
        }

        [Test, Timeout(5000)]
        public void TestEchoMessage()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                Random rd = new Random(1);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < 100; i++)
                {
                    sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                    string command = sb.ToString();
                    Console.WriteLine("Client prepare sent:" + command);
                    socket.SendTo(Encoding.UTF8.GetBytes("ECHO " + command + "\r\n"), serverAddress);
                    Console.WriteLine("Client sent:" + command);
                    string echoMessage = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                    Console.WriteLine("C:" + echoMessage);
                    Assert.AreEqual(command, echoMessage);
                }
            }
        }

        private List<byte> ReceiveMessage(Socket socket, EndPoint serverAddress)
        {
            int length = 1024;
            byte[] buffer = new byte[length];
            int read = socket.ReceiveFrom(buffer, ref serverAddress);
            if (read < length)
                return buffer.Take(read).ToList();
            else
            {
                var total = buffer.ToList();
                total.AddRange(ReceiveMessage(socket, serverAddress));
                return total;
            }
        }
    }
}
