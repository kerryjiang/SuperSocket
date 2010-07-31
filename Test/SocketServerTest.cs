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
        private static Dictionary<IServerConfig, TestServer[]> m_Servers = new Dictionary<IServerConfig, TestServer[]>();

        private readonly IServerConfig m_Config;

        public SocketServerTest(IServerConfig config)
        {
            m_Config = config;
        }

        private TestServer GetServerByIndex(int index)
        {
            TestServer[] servers = new TestServer[0];

            if (!m_Servers.TryGetValue(m_Config, out servers))
                return null;

            return servers[index];
        }

        private TestServer ServerX
        {
            get
            {
                return GetServerByIndex(0);
            }
        }

        private TestServer ServerY
        {
            get
            {
                return GetServerByIndex(1);
            }
        }

        private TestServer ServerZ
        {
            get
            {
                return GetServerByIndex(2);
            }
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            if (m_Servers.ContainsKey(m_Config))
                return;

            var serverX = new TestServer();
            serverX.Setup(string.Empty, m_Config, string.Empty);

            var serverY = new TestServer(new TestCommandParser());
            serverY.Setup(string.Empty, m_Config, string.Empty);

            var serverZ = new TestServer(new TestCommandParser(), new TestCommandParameterParser());
            serverZ.Setup(string.Empty, m_Config, string.Empty);

            m_Servers[m_Config] = new TestServer[]
            {
                serverX,
                serverY,
                serverZ
            };

        }

        [TestFixtureTearDown]
        public void StopAllServers()
        {
            StopServer();
        }

        [Test, Repeat(10)]
        public void TestStartStop()
        {
            StartServer();
            Assert.IsTrue(ServerX.IsRunning);
            StopServer();
            Assert.IsFalse(ServerX.IsRunning);
        }

        private bool CanConnect()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

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
            if (ServerX.IsRunning)
                ServerX.Stop();

            ServerX.Start();
            Console.WriteLine("Socket server X has been started!");
        }

        [TearDown]
        public void StopServer()
        {
            if (ServerX.IsRunning)
            {
                ServerX.Stop();
                Console.WriteLine("Socket server X has been stopped!");
            }

            if (ServerY != null && ServerY.IsRunning)
            {
                ServerY.Stop();
                Console.WriteLine("Socket server Y has been stopped!");
            }

            if (ServerZ != null && ServerZ.IsRunning)
            {
                ServerZ.Stop();
                Console.WriteLine("Socket server Z has been stopped!");
            }
        }

        [Test, Repeat(10)]
        public void TestWelcomeMessage()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, m_Config.Name), welcomeString);
                }
            }
        }

        [Test]
        public void TestEchoMessage()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();

                    Console.WriteLine("Welcome: " + welcomeString);

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
                        Console.WriteLine("C:" + echoMessage);
                        Assert.AreEqual(command, echoMessage);
                    }
                }
            }
        }

        [Test, Repeat(5)]
        public void TestCommandParser()
        {
            if (ServerY.IsRunning)
                ServerY.Stop();

            ServerY.Start();
            Console.WriteLine("Socket server Y has been started!");

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

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

        [Test, Repeat(5)]
        public void TestCommandParameterParser()
        {
            if (ServerZ.IsRunning)
                ServerZ.Stop();

            ServerZ.Start();
            Console.WriteLine("Socket server Z has been started!");

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

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

                    List<string> received = new List<string>();

                    foreach (var p in arrParam)
                    {
                        string r = reader.ReadLine();
                        Console.WriteLine("C: " + r);
                        received.Add(r);
                    }

                    Assert.AreEqual(arrParam, received);
                }
            }
        }

        [Test, Repeat(3)]
        public void TestReceiveInLength()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

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
                    socketStream.Flush();
                    socketStream.Flush();

                    Thread.Sleep(1000);

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
