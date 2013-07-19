using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;
using SuperSocket.Test.Command;


namespace SuperSocket.Test
{
    public abstract class SocketServerTest : BootstrapTestBase
    {
        protected Encoding m_Encoding;

        public SocketServerTest()
        {
            m_Encoding = new UTF8Encoding();
        }

        protected abstract string DefaultServerConfig { get; }

        [Test, Repeat(3)]
        public void TestStartStop()
        {
            SetupBootstrap(DefaultServerConfig);
            var bootstrap = BootStrap;
            var server = bootstrap.GetServerByName("TestServer");

            bootstrap.Start();
            Assert.IsTrue(server.State == ServerState.Running);

            bootstrap.Stop();
            Assert.IsTrue(server.State == ServerState.NotStarted);

            bootstrap.Start();
            Assert.IsTrue(server.State == ServerState.Running);
        }

        private bool CanConnect(int port)
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

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
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, serverConfig.Name), welcomeString);
                }
            }
        }

        [Test]
        public void TestSessionConnectedState()
        {
            byte[] data = new byte[] { 0xff, 0xff, 0xff, 0xf0 };

            Console.WriteLine(BitConverter.ToInt32(data, 0));

            var configSource = SetupBootstrap(DefaultServerConfig);

            if (configSource.Isolation != IsolationMode.None)
                return;

            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            BootStrap.Start();

            var server = BootStrap.AppServers.FirstOrDefault() as IAppServer;

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();
                    writer.WriteLine("SESS");
                    writer.Flush();

                    var sessionID = reader.ReadLine();
                    var session = server.GetAppSessionByID(sessionID);

                    if (session == null)
                        Assert.Fail("Failed to get session by sessionID");

                    Assert.IsTrue(session.Connected);

                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                    finally
                    {
                        try
                        {
                            socket.Close();
                        }
                        catch { }
                    }

                    while (true)
                    {
                        Thread.Sleep(5000);

                        if (!session.Connected)
                            break;
                    }
                }
            }
        }

        private bool TryConnect(EndPoint serverAddress)
        {
            var task = Task.Factory.StartNew(() =>
                {
                    using (Socket trySocket = CreateClientSocket())
                    {
                        trySocket.Connect(serverAddress);
                        var innerSocketStream = GetSocketStream(trySocket);
                        innerSocketStream.ReadTimeout = 500;

                        using (var tryReader = new StreamReader(innerSocketStream, m_Encoding, true))
                        {
                            return tryReader.ReadLine() != null;
                        }
                    }
                });

            try
            {
                if (!task.Wait(1500))
                    return false;
            }
            catch
            {
                return false;
            }

            return task.Result;
        }

        private bool TestMaxConnectionNumber(int maxConnectionNumber)
        {
            var configSource = SetupBootstrap(DefaultServerConfig, new Func<IServerConfig, IServerConfig>(c =>
                {
                    var nc = new ServerConfig(c);
                    nc.MaxConnectionNumber = maxConnectionNumber;
                    return nc;
                }));

            BootStrap.Start();

            var config = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), config.Port);

            List<Socket> sockets = new List<Socket>();

            var server = BootStrap.AppServers.FirstOrDefault();

            try
            {
                for (int i = 0; i < maxConnectionNumber; i++)
                {
                    Socket socket = CreateClientSocket();
                    socket.Connect(serverAddress);
                    Stream socketStream = GetSocketStream(socket);
                    StreamReader reader = new StreamReader(socketStream, m_Encoding, true);
                    reader.ReadLine();
                    sockets.Add(socket);
                }

                Assert.AreEqual(maxConnectionNumber, server.SessionCount);

                Assert.IsFalse(TryConnect(serverAddress));

                sockets[0].SafeClose();

                Thread.Sleep(500);

                Assert.AreEqual(maxConnectionNumber - 1, server.SessionCount);

                return TryConnect(serverAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                throw e;
            }
            finally
            {
                ClearBootstrap();
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
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    for (int i = 0; i < 10; i++)
                    {
                        string commandName = Guid.NewGuid().ToString().Substring(i, 3);

                        if(commandName.Equals("325"))
                            continue;

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
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();

                    Console.WriteLine("Welcome: " + welcomeString);

                    char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                    Random rd = new Random(1);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < 1; i++)
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
        public void TestSendBeforeClose()
        {
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    var line = Guid.NewGuid().ToString();

                    writer.WriteLine("CLOSE " + line);
                    writer.Flush();

                    string echoMessage = reader.ReadLine();
                    Console.WriteLine("C:" + echoMessage);
                    Assert.AreEqual(line, echoMessage);
                }
            }
        }

        [Test]
        public void TestCommandCombining()
        {
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
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
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
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

        private bool RunEchoMessage(int port, int runIndex)
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            Socket socket = CreateClientSocket();
            socket.Connect(serverAddress);
            Stream socketStream = GetSocketStream(socket);
            using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
            using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
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
            var configSource = StartBootstrap(DefaultServerConfig, c => new ServerConfig(c)
                {
                    IdleSessionTimeOut = 180
                });
            var serverConfig = configSource.Servers.FirstOrDefault();

            int concurrencyCount = 100;

            Semaphore semaphore = new Semaphore(0, concurrencyCount);

            bool[] resultArray = new bool[concurrencyCount];

            System.Threading.Tasks.Parallel.For(0, concurrencyCount, i =>
            {
                resultArray[i] = RunEchoMessage(serverConfig.Port, i);
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
        public virtual void TestClearTimeoutSession()
        {
            var configSource = StartBootstrap(DefaultServerConfig, c => new ServerConfig(c)
            {
                ClearIdleSession = true
            });

            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            var server = BootStrap.AppServers.FirstOrDefault();

            Socket socket = CreateClientSocket();
            socket.Connect(serverAddress);
            Stream socketStream = GetSocketStream(socket);
            using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
            using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
            {
                reader.ReadLine();
            }

            Assert.AreEqual(1, server.SessionCount);
            Thread.Sleep(2000);
            Assert.AreEqual(1, server.SessionCount);
            Thread.Sleep(5000);
            Assert.AreEqual(0, server.SessionCount);
        }

        [Test]
        public void TestCustomCommandName()
        {
            var configSource = StartBootstrap(DefaultServerConfig);
            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
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
            var configSource = StartBootstrap(DefaultServerConfig, c =>
                new ServerConfig(c)
                {
                    ServerType = "SuperSocket.Test.TestServerWithCustomRequestFilter, SuperSocket.Test"
                });

            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
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

        [Test]
        public void TestCloseFromClient()
        {
            var configSource = StartBootstrap(DefaultServerConfig, c =>
                new ServerConfig(c)
                {
                    LogBasicSessionActivity = false,
                    ClearIdleSession = false,
                    MaxConnectionNumber = 1000,
                });

            var n = 100;
            var sockets = new List<Socket>(n);
            var streams = new List<Stream>(n);
            var server = BootStrap.AppServers.FirstOrDefault();

            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            for (var i = 0; i < n; i++)
            {
                Socket socket = CreateClientSocket();
                socket.Connect(serverAddress);
                sockets.Add(socket);
                streams.Add(GetSocketStream(socket));
            }

            Console.WriteLine(sockets.Count);

            int waitRound = 10;

            while (waitRound > 0)
            {
                Thread.Sleep(1000);

                waitRound--;

                if (n == server.SessionCount)
                    break;
            }

            Assert.AreEqual(n, server.SessionCount);

            for (var i = n - 1; i >= 0; i--)
            {
                var s = sockets[i];

                s.SafeClose();

                Thread.Sleep(100);

                if (i != server.SessionCount)
                {
                    Thread.Sleep(500);
                    Assert.AreEqual(i, server.SessionCount);
                }
                
                Console.WriteLine("Conn: {0}", i);
            }
        }

        [Test]
        public void TestCloseFromServer()
        {
            var configSource = StartBootstrap(DefaultServerConfig, c =>
                new ServerConfig(c)
                {
                    LogBasicSessionActivity = false,
                    ClearIdleSession = false,
                    MaxConnectionNumber = 1000,
                });

            var n = 100;
            var sockets = new List<Socket>(n);
            var streams = new List<Stream>(n);
            var server = BootStrap.AppServers.FirstOrDefault();

            var serverConfig = configSource.Servers.FirstOrDefault();
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            for (var i = 0; i < n; i++)
            {
                Socket socket = CreateClientSocket();
                socket.Connect(serverAddress);
                sockets.Add(socket);
                streams.Add(GetSocketStream(socket));
            }

            Console.WriteLine(sockets.Count);
            Thread.Sleep(500);
            Assert.AreEqual(n, server.SessionCount);

            for (var i = n - 1; i >= 0; i--)
            {
                Stream socketStream = streams[i];
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    writer.WriteLine("CLOSE");
                    writer.Flush();
                }

                Thread.Sleep(100);

                if (i != server.SessionCount)
                {
                    Thread.Sleep(500);
                    Assert.AreEqual(i, server.SessionCount);
                }

                Console.WriteLine("Conn: {0}", i);
            }
        }

        [Test, Repeat(5)]
        public void TestConcurrentSending()
        {
            var configSource = StartBootstrap(DefaultServerConfig, c => new ServerConfig(c)
                {
                    SendingQueueSize = 100
                });

            var serverConfig = configSource.Servers.FirstOrDefault();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            string[] source = SEND.GetStringSource();

            string[] received = new string[source.Length];

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    for (var j = 0; j < 1000; j++)
                    {
                        writer.WriteLine("SEND");
                        writer.Flush();

                        int i;

                        for (i = 0; i < received.Length; i++)
                        {
                            var line = reader.ReadLine();
                            if (line == null)
                                break;
                            received[i] = line;
                        }

                        Assert.AreEqual(received.Length, i);
                        Console.WriteLine("ROUND");
                    }
                }
            }

            if (received.Distinct().Count() != received.Length)
                Assert.Fail("Duplicated record!");

            var dict = source.ToDictionary(i => i);

            for (var i = 0; i < received.Length; i++)
            {
                if (!dict.Remove(received[i]))
                    Assert.Fail(received[i]);
            }

            if (dict.Count > 0)
                Assert.Fail();
        }


        [Test]
        public void TestFastSending()
        {
            var configSource = StartBootstrap(DefaultServerConfig, c => new ServerConfig(c)
                {
                    LogBasicSessionActivity = false,
                    SendingQueueSize = 100
                });

            var serverConfig = configSource.Servers.FirstOrDefault();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = CreateClientSocket())
            {
                socket.Connect(serverAddress);
                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    string sendLine;

                    int i = 0;
                    int testRound = 50000;

                    while (i < testRound)
                    {
                        sendLine = Guid.NewGuid().ToString();
                        writer.Write("ECHO " + sendLine + "\r\n");
                        writer.Flush();

                        var line = reader.ReadLine();
                        if (line == null)
                        {
                            i++;
                            break;
                        }

                        Assert.AreEqual(sendLine, line);
                        i++;
                    }

                    Console.WriteLine("Client sent: {0}", i);
                    Console.WriteLine("Server sent: {0}", ECHO.Sent);
                    Assert.AreEqual(testRound, i);
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
