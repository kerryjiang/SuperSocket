using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Config;


namespace SuperSocket.Test
{
    public abstract class SocketServerTest
    {
        private TestServer m_Server;
        private readonly int m_Port;
        private readonly string m_ServerName;
        private readonly SocketMode m_SocketMode = SocketMode.Sync;

        public SocketServerTest(string serverName, int port, SocketMode mode)
        {
            m_ServerName = serverName;
            m_Port = port;
            m_SocketMode = mode;
        }

        [SetUp]
        public void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            ServerConfig config = GetServerConfig();

            m_Server = new TestServer();
            m_Server.Setup(string.Empty, config, string.Empty);
        }

        private ServerConfig GetServerConfig()
        {
            ServerConfig config = new ServerConfig();
            config.Name = m_ServerName;
            config.Ip = "Any";
            config.MaxConnectionNumber = 1;
            config.Port = m_Port;
            config.Mode = m_SocketMode;
            return config;
        }

        [Test]
        public void TestStartStop()
        {
            StartServer();
            Thread.Sleep(1000);
            Assert.IsTrue(CanConnect());
            StopServer();
            Thread.Sleep(1000);
            Assert.IsFalse(CanConnect());
        }

        private bool CanConnect()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(serverAddress);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }                
            }
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

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, m_ServerName), welcomeString);
                }
            }

            StopServer();
        }

        [Test]
        public void TestEchoMessage()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    string command = string.Format("Hello World ({0})!", Guid.NewGuid().ToString());
                    writer.WriteLine("ECHO " + command);
                    writer.Flush();
                    string echoMessage = reader.ReadLine();
                    Assert.AreEqual(command, echoMessage);
                }
            }

            StopServer();
        }

        [Test]
        public void TestCommandParser()
        {
            ServerConfig config = GetServerConfig();

            m_Server = new TestServer(new TestCommandParser());
            m_Server.Setup(string.Empty, config, string.Empty);
            m_Server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    string command = string.Format("Hello World ({0})!", Guid.NewGuid().ToString());
                    writer.WriteLine("ECHO:" + command);
                    writer.Flush();
                    string echoMessage = reader.ReadLine();
                    Assert.AreEqual(command, echoMessage);
                }
            }

            m_Server.Stop();
        }

        [Test]
        public void TestCommandParameterParser()
        {
            ServerConfig config = GetServerConfig();

            m_Server = new TestServer(new TestCommandParser(), new TestCommandParameterParser());
            m_Server.Setup(string.Empty, config, string.Empty);
            m_Server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    reader.ReadLine();
                    string command = string.Format("Hello World ({0})!", Guid.NewGuid().ToString());
                    string[] arrParam = new string[] { "A1", "A2", "A4", "B2", "A6", "E5" };
                    writer.WriteLine("PARA:" + string.Join(",", arrParam));
                    writer.Flush();

                    foreach (var p in arrParam)
                    {
                        string r = reader.ReadLine();
                        Console.WriteLine(r);
                        Assert.AreEqual(p, r);
                    }
                }
            }

            m_Server.Stop();
        }
    }
}
