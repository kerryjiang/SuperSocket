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
        private TestServer m_ServerX;
        private readonly int m_Port;
        private readonly string m_ServerName;
        private readonly SocketMode m_SocketMode = SocketMode.Sync;

        public SocketServerTest(string serverName, int port, SocketMode mode)
        {
            m_ServerName = serverName;
            m_Port = port;
            m_SocketMode = mode;
        }

        [TestFixtureSetUp]
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

        [Test, Timeout(4000)]
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
            if (m_Server.IsRunning)
                m_Server.Stop();

            m_Server.Start();
            Console.WriteLine("Socket server has been started!");
        }

        [TearDown]
        public void StopServer()
        {
            if (m_Server.IsRunning)
            {
                m_Server.Stop();
                Console.WriteLine("Socket server has been stopped!");
            }

            if (m_ServerX != null && m_ServerX.IsRunning)
            {
                m_ServerX.Stop();
                Console.WriteLine("Socket server X has been stopped!");
            }
        }

        [Test, Timeout(2000)]
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
        }

        [Test, Timeout(30000)]
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

                    char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                    Random rd = new Random(1);

                    StringBuilder sb = new StringBuilder();                   

                    for (int i = 0; i < 100; i++)
                    {
                        sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                        string command = sb.ToString();
                        writer.WriteLine("ECHO " + command);
                        writer.Flush();
                        string echoMessage = reader.ReadLine();
                        Console.WriteLine(echoMessage);
                        Assert.AreEqual(command, echoMessage);
                    }
                }
            }
        }

        [Test, Timeout(2000)]
        public void TestCommandParser()
        {
            ServerConfig config = GetServerConfig();

            m_ServerX = new TestServer(new TestCommandParser());
            m_ServerX.Setup(string.Empty, config, string.Empty);
            m_ServerX.Start();

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
        }

        [Test, Timeout(2000)]
        public void TestCommandParameterParser()
        {
            ServerConfig config = GetServerConfig();

            m_ServerX = new TestServer(new TestCommandParser(), new TestCommandParameterParser());
            m_ServerX.Setup(string.Empty, config, string.Empty);
            m_ServerX.Start();

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
        }

        [Test, Timeout(5000)]
        public void TestReceiveInLength()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                {
                    reader.ReadLine();

                    Stream testStream = this.GetType().Assembly.GetManifestResourceStream("SuperSocket.Test.Resources.TestFile.txt");
                    byte[] data = ReadStreamToBytes(testStream);

                    byte[] cmdData = Encoding.Default.GetBytes("RECEL " + data.Length + Environment.NewLine);

                    socketStream.Write(cmdData, 0, cmdData.Length);
                    socketStream.Write(data, 0, data.Length);
                    socketStream.Flush();

                    Thread.Sleep(1000);

                    MemoryStream ms = new MemoryStream();

                    while (true)
                    {
                        string received = reader.ReadLine();

                        received += Environment.NewLine;
                        byte[] temp = Encoding.Default.GetBytes(received);
                        ms.Write(temp, 0, temp.Length);

                        if (reader.Peek() < 0)
                            break;
                    }

                    byte[] receivedData = ms.ToArray();
                    Assert.AreEqual(data, receivedData);
                }
            }            
        }

        private byte[] ReadStreamToBytes(Stream stream)
        {
            MemoryStream ms = new MemoryStream();

            byte[] buffer = new byte[1024 * 10];

            while (true)
            {
                int read = stream.Read(buffer, 0, buffer.Length);

                if (read <= 0)
                    break;

                ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
        }        
    }
}
