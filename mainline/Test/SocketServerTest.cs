using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Config;


namespace SuperSocket.Test
{
    public abstract class SocketServerTest
    {
        private static Dictionary<IServerConfig, TestServer[]> m_Servers = new Dictionary<IServerConfig, TestServer[]>();

        private readonly IServerConfig m_Config;

        public SocketServerTest()
        {
            m_Config = DefaultServerConfig;
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

        protected abstract IServerConfig DefaultServerConfig { get; }


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

        [Test, Repeat(3)]
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

        private void TestMaxConnectionNumber(int maxConnectionNumber)
        {
            var server = new TestServer();
            var defaultConfig = DefaultServerConfig;

            var config = new ServerConfig
            {
                Ip = defaultConfig.Ip,
                LogCommand = defaultConfig.LogCommand,
                MaxConnectionNumber = maxConnectionNumber,
                Mode = defaultConfig.Mode,
                Name = defaultConfig.Name,
                Port = defaultConfig.Port
            };

            server.Setup(string.Empty, config, string.Empty);

            List<Socket> sockets = new List<Socket>();

            try
            {
                server.Start();

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

                for (int i = 0; i < maxConnectionNumber; i++)
                {
                    Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(serverAddress);
                    Stream socketStream = new NetworkStream(socket);
                    StreamReader reader = new StreamReader(socketStream, Encoding.Default, true);
                    reader.ReadLine();
                    sockets.Add(socket);
                }

                using (Socket trySocket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    Console.WriteLine("Start to connect try socket");
                    trySocket.Connect(serverAddress);
                    var innerSocketStream = new NetworkStream(trySocket);
                    innerSocketStream.ReadTimeout = 500;

                    using (StreamReader tryReader = new StreamReader(innerSocketStream, Encoding.Default, true))
                    {
                        Assert.Throws<IOException>(delegate
                        {
                            string welcome = tryReader.ReadLine();
                            Console.WriteLine(welcome);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
            finally
            {
                sockets.ForEach(s =>
                {
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                });

                server.Stop();
            }
        }

        [Test]
        public void TestMaxConnectionNumber()
        {
            TestMaxConnectionNumber(1);
            TestMaxConnectionNumber(2);
            TestMaxConnectionNumber(5);
            TestMaxConnectionNumber(15);
        }

        [Test, Repeat(2)]
        public void TestUnknownCommand()
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
                    reader.ReadLine();

                    for (int i = 0; i < 10; i++)
                    {
                        string commandName = Guid.NewGuid().ToString().Substring(0, 3);
                        string command = commandName + " " + DateTime.Now;
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Console.WriteLine(line);
                        Assert.AreEqual(string.Format(TestSession.UnknownCommandMessageFormat, commandName), line);
                    }                        
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

        [Test, Repeat(3)]
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

        [Test, Repeat(3)]
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

            ServerZ.Stop();
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

        [Test, Repeat(3)]
        public void TestReceiveByMark()
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
                    byte[] data = ReadStreamToBytes(testStream, Encoding.ASCII.GetBytes(string.Format("{0}.{0}", Environment.NewLine)));

                    byte[] cmdData = Encoding.Default.GetBytes("RECEM" + Environment.NewLine);

                    socketStream.Write(cmdData, 0, cmdData.Length);
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
            return ReadStreamToBytes(stream, null);
        }

        private byte[] ReadStreamToBytes(Stream stream, byte[] endMark)
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

            if (endMark != null && endMark.Length > 0)
                ms.Write(endMark, 0, endMark.Length);

            return ms.ToArray();
        }        
    }
}
