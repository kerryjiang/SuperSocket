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
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.Common.Logging;


namespace SuperSocket.Test
{
    public abstract class SocketServerTest
    {
        private static Dictionary<IServerConfig, TestServer[]> m_Servers = new Dictionary<IServerConfig, TestServer[]>();

        private readonly IServerConfig m_Config;

        private IRootConfig m_RootConfig;
        
        private Encoding m_Encoding;

        public SocketServerTest()
        {
            m_Config = DefaultServerConfig;
            m_RootConfig = new RootConfig
            {
                MaxWorkingThreads = 500,
                MaxCompletionPortThreads = 500,
                MinWorkingThreads = 5,
                MinCompletionPortThreads = 5,
                DisablePerformanceDataCollector = true
            };
            
            m_Encoding = new UTF8Encoding();
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

        protected abstract IServerConfig DefaultServerConfig { get; }

        [TestFixtureSetUp]
        public void Setup()
        {
            if (m_Servers.ContainsKey(m_Config))
                return;

            var serverX = new TestServer();
            var serverY = new TestServer(new TestCommandParser());

            IBootstrap bootstrap = new DefaultBootstrap();
            bootstrap.Initialize(m_RootConfig, new IAppServer[] { serverX, serverY }, new IServerConfig[] { m_Config, m_Config });

            m_Servers[m_Config] = new TestServer[]
            {
                serverX,
                serverY
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

            using (Socket socket = CreateClientSocket())
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
        }

        protected virtual Socket CreateClientSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        protected virtual Stream GetSocketStream(Socket socket)
        {
            return new NetworkStream(socket);
        }

        [Test, Repeat(3)]
        public void TestWelcomeMessage()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, m_Config.Name), welcomeString);
                }
            }
        }

        private bool TestMaxConnectionNumber(int maxConnectionNumber)
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
                Port = defaultConfig.Port,
                Security = defaultConfig.Security,
                Certificate = defaultConfig.Certificate
            };

            var bootstrap = new DefaultBootstrap();

            bootstrap.Initialize(m_RootConfig, new IAppServer[] { server }, new IServerConfig[] { config });

            List<Socket> sockets = new List<Socket>();

            try
            {
                bootstrap.Start();

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

                for (int i = 0; i < maxConnectionNumber; i++)
                {
                    Socket socket = CreateClientSocket();
                    socket.Connect(serverAddress);
                    Stream socketStream = GetSocketStream(socket);
                    StreamReader reader = new StreamReader(socketStream, m_Encoding, true);
                    reader.ReadLine();
                    sockets.Add(socket);
                }

                try
                {
                    using (Socket trySocket = CreateClientSocket())
                    {
                        trySocket.Connect(serverAddress);
                        var innerSocketStream = new NetworkStream(trySocket);
                        innerSocketStream.ReadTimeout = 500;

                        using (StreamReader tryReader = new StreamReader(innerSocketStream, m_Encoding, true))
                        {
                            string welcome = tryReader.ReadLine();
                            Console.WriteLine(welcome);
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                return false;
            }
            finally
            {
                bootstrap.Stop();
            }
        }

        [Test, Category("Concurrency")]
        public void TestMaxConnectionNumber()
        {
            Assert.IsTrue(TestMaxConnectionNumber(1));
            Assert.IsTrue(TestMaxConnectionNumber(2));
            Assert.IsTrue(TestMaxConnectionNumber(5));
            Assert.IsTrue(TestMaxConnectionNumber(15));
        }

        [Test, Repeat(2)]
        public void TestUnknownCommand()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    for (int i = 0; i < 10; i++)
                    {
                        string commandName = Guid.NewGuid().ToString().Substring(i, 3);
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

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
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


        [Test]
        public void TestCommandCombining()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();

                    Console.WriteLine("Welcome: " + welcomeString);

                    char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                    Random rd = new Random(1);

                    for (int j = 0; j < 10; j++)
                    {
                        StringBuilder sb = new StringBuilder();

                        List<string> source = new List<string>(5);

                        for (int i = 0; i < 5; i++)
                        {
                            sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                            string command = sb.ToString();
                            source.Add(command);
                            writer.WriteLine("ECHO " + command);
                        }

                        writer.Flush();

                        for (int i = 0; i < 5; i++)
                        {
                            string line = reader.ReadLine();
                            Assert.AreEqual(source[i], line);
                        }
                    }
                }
            }
        }


        [Test]
        public void TestBrokenCommandBlock()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();

                    Console.WriteLine("Welcome: " + welcomeString);

                    char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                    Random rd = new Random(1);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < 50; i++)
                    {
                        sb.Append(chars[rd.Next(0, chars.Length - 1)]);                        
                    }

                    string command = sb.ToString();

                    var commandSource = ("ECHO " + command).ToList();

                    while (commandSource.Count > 0)
                    {
                        int readLen = rd.Next(1, commandSource.Count);
                        writer.Write(commandSource.Take(readLen).ToArray());
                        Console.WriteLine(commandSource.Take(readLen).ToArray());
                        writer.Flush();
                        commandSource.RemoveRange(0, readLen);
                        Thread.Sleep(200);
                    }

                    writer.WriteLine();
                    writer.Flush();
                  
                    string echoMessage = reader.ReadLine();
                    Console.WriteLine("C:" + echoMessage);
                    Assert.AreEqual(command, echoMessage);
                }
            }
        }

        private bool RunEchoMessage(int runIndex)
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            Socket socket = CreateClientSocket();
            socket.Connect(serverAddress);
            Stream socketStream = GetSocketStream(socket);
            using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
            using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
            {
                string welcomeString = reader.ReadLine();
                Console.WriteLine(welcomeString);

                char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                Random rd = new Random(1);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < 10; i++)
                {
                    sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                    string command = sb.ToString();
                    writer.WriteLine("ECHO " + command);
                    writer.Flush();
                    string echoMessage = reader.ReadLine();
                    if (!string.Equals(command, echoMessage))
                    {
                        return false;
                    }
                    Thread.Sleep(50);
                }
            }

            return true;
        }

        [Test, Repeat(3), Category("Concurrency")]
        public void TestConcurrencyCommunication()
        {
            StartServer();

            int concurrencyCount = 100;

            Semaphore semaphore = new Semaphore(0, concurrencyCount);

            bool[] resultArray = new bool[concurrencyCount];

            System.Threading.Tasks.Parallel.For(0, concurrencyCount, i =>
            {
                resultArray[i] = RunEchoMessage(i);
                semaphore.Release();
            });

            for (var i = 0; i < concurrencyCount; i++)
            {
                semaphore.WaitOne();
                Console.WriteLine("Got " + i);
            }

            bool result = true;

            for (var i = 0; i < concurrencyCount; i++)
            {
                Console.WriteLine(i + ":" + resultArray[i]);
                if (!resultArray[i])
                    result = false;
            }

            if (!result)
                Assert.Fail("Concurrent Communications fault!");
        }

        [Test, Repeat(5)]
        public void TestClearTimeoutSession()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            Socket socket = CreateClientSocket();
            socket.Connect(serverAddress);
            Stream socketStream = GetSocketStream(socket);
            using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
            using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
            {
                reader.ReadLine();
            }

            Assert.AreEqual(1, ServerX.SessionCount);
            Thread.Sleep(2000);
            Assert.AreEqual(1, ServerX.SessionCount);
            Thread.Sleep(5000);
            Assert.AreEqual(0, ServerX.SessionCount);
        }

        [Test]
        public void TestCustomCommandName()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();
                    string param = Guid.NewGuid().ToString();
                    writer.WriteLine("325 " + param);
                    writer.Flush();
                    string echoMessage = reader.ReadLine();
                    Console.WriteLine("C:" + echoMessage);
                    Assert.AreEqual(string.Format(SuperSocket.Test.Command.NUM.ReplyFormat, param), echoMessage);
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

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();
                    string command = string.Format("Hello World ({0})!", Guid.NewGuid().ToString());
                    writer.WriteLine("ECHO:" + command);
                    writer.Flush();
                    string echoMessage = reader.ReadLine();
                    Assert.AreEqual(command, echoMessage);
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
