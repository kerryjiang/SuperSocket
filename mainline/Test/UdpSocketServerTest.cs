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
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine;
using SuperSocket.Test.Udp;

namespace SuperSocket.Test
{
    public class MyUdpRequestInfo : UdpRequestInfo
    {
        public MyUdpRequestInfo(string key, string sessionID)
            : base(key, sessionID)
        {

        }

        public string Value { get; set; }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.AddRange(Encoding.ASCII.GetBytes(Key));
            data.AddRange(Encoding.ASCII.GetBytes(SessionID));

            int expectedLen = 36 + 4;

            if (data.Count < expectedLen)
            {
                for (var i = 0; i < expectedLen - data.Count; i++)
                {
                    data.Add(0x00);
                }
            }

            data.AddRange(Encoding.UTF8.GetBytes(Value));

            return data.ToArray();
        }
    }

    [TestFixture]
    public class UdpSocketServerTest
    {
        private TestServer m_Server;
        private IServerConfig m_Config;
        private IRootConfig m_RootConfig;
        private Encoding m_Encoding;

        protected IServerConfig DefaultServerConfig
        {
            get
            {
                return new ServerConfig
                    {
                        Ip = "127.0.0.1",
                        LogCommand = true,
                        MaxConnectionNumber = 100,
                        Mode = SocketMode.Udp,
                        Name = "Udp Test Socket Server",
                        Port = 2196,
                        ClearIdleSession = true,
                        ClearIdleSessionInterval = 1,
                        IdleSessionTimeOut = 5
                    };
            }
        }

        public UdpSocketServerTest()
        {
            m_Config = DefaultServerConfig;
            m_RootConfig = new RootConfig();
            m_Encoding = new System.Text.UTF8Encoding();
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            m_Server = new TestServer();

            IBootstrap bootstrap = new DefaultBootstrap();
            bootstrap.Initialize(m_RootConfig, new IAppServer[] { m_Server }, new IServerConfig[] { m_Config });
        }

        [TestFixtureTearDown]
        [TearDown]
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
            m_Server.Start();
            Console.WriteLine("The UDP Socket server is started!");
        }

        private void StopServer()
        {
            if (m_Server.IsRunning)
            {
                m_Server.Stop();
                Console.WriteLine("The UDP Socket server is stopped!");
            }
        }

        [Test, Repeat(5)]
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
                    socket.SendTo(m_Encoding.GetBytes("ECHO " + command + Environment.NewLine), serverAddress);
                    Console.WriteLine("Client sent:" + command);
                    string echoMessage = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                    Console.WriteLine("C:" + echoMessage);
                    Assert.AreEqual(command, echoMessage);
                }
            }
        }

        [Test, Timeout(5000)]
        public void TestUdpCommand()
        {
            var testServer = new UdpAppServer();

            IBootstrap bootstrap = new DefaultBootstrap();
            bootstrap.Initialize(m_RootConfig, new IAppServer[] { testServer }, new IServerConfig[] { m_Config });

            testServer.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                Random rd = new Random(1);

                StringBuilder sb = new StringBuilder();

                var sessionID = Guid.NewGuid().ToString();

                for (int i = 0; i < 100; i++)
                {
                    sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                    string command = sb.ToString();

                    Console.WriteLine("Client prepare sent:" + command);

                    var cmdInfo = new MyUdpRequestInfo("SESS", sessionID);
                    cmdInfo.Value = command;

                    socket.SendTo(cmdInfo.ToData(), serverAddress);

                    Console.WriteLine("Client sent:" + command);

                    string[] res = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray()).Split(' ');

                    Assert.AreEqual(sessionID, res[0]);
                    Assert.AreEqual(command, res[1]);
                }
            }

            testServer.Stop();
        }

        [Test]
        public void TestCommandCombining()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                Random rd = new Random(1);

                for (int j = 0; j < 10; j++)
                {
                    StringBuilder sb = new StringBuilder();

                    List<string> source = new List<string>(5);

                    StringBuilder sbCombile = new StringBuilder();

                    for (int i = 0; i < 5; i++)
                    {
                        sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                        string command = sb.ToString();
                        source.Add(command);
                        sbCombile.AppendLine("ECHO " + command);
                    }

                    socket.SendTo(m_Encoding.GetBytes(sbCombile.ToString()), serverAddress);                    

                    for (int i = 0; i < 5; i++)
                    {
                        byte[] receivedData = ReceiveMessage(socket, serverAddress).ToArray();
                        string receivedContent = m_Encoding.GetString(receivedData);
                        StringReader reader = new StringReader(receivedContent);
                        string line = reader.ReadLine();
                        Assert.AreEqual(source[i], line);
                    }
                }
            }
        }

        //[Test]
        //public void TestBrokenCommandBlock()
        //{
        //    StartServer();

        //    EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

        //    using (Socket socket = CreateClientSocket())
        //    {
        //        char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

        //        Random rd = new Random(1);

        //        StringBuilder sb = new StringBuilder();

        //        for (int i = 0; i < 50; i++)
        //        {
        //            sb.Append(chars[rd.Next(0, chars.Length - 1)]);
        //        }

        //        string command = sb.ToString();

        //        var commandSource = ("ECHO " + command).ToList();

        //        while (commandSource.Count > 0)
        //        {
        //            int readLen = rd.Next(1, commandSource.Count);
        //            Console.WriteLine(commandSource.Take(readLen).ToArray());
        //            socket.SendTo(m_Encoding.GetBytes(commandSource.Take(readLen).ToArray()), serverAddress);
        //            commandSource.RemoveRange(0, readLen);
        //        }

        //        socket.SendTo(m_Encoding.GetBytes(Environment.NewLine), serverAddress);

        //        string echoMessage = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
        //        Console.WriteLine("C:" + echoMessage);
        //        Assert.AreEqual(command, echoMessage);
        //    }
        //}

        private bool RunEchoMessage()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                Random rd = new Random(1);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < 10; i++)
                {
                    sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                    string command = sb.ToString();
                    socket.SendTo(m_Encoding.GetBytes("ECHO " + command + Environment.NewLine), serverAddress);
                    string echoMessage = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                    Assert.AreEqual(command, echoMessage);
                    if (!string.Equals(command, echoMessage))
                        return false;
                    Thread.Sleep(100);
                }
            }

            return true;
        }

        [Test, Repeat(3)]
        public void TestConcurrencyCommunication()
        {
            StartServer();

            int concurrencyCount = 64;

            Semaphore semaphore = new Semaphore(0, concurrencyCount);

            ManualResetEvent taskEvent = new ManualResetEvent(true);

            System.Threading.Tasks.Parallel.For(0, concurrencyCount, i =>
                {
                    if (!RunEchoMessage())
                        taskEvent.Reset();
                    semaphore.Release();
                });

            for (var i = 0; i < concurrencyCount; i++)
            {
                semaphore.WaitOne();
            }

            if(!taskEvent.WaitOne(1000))
                Assert.Fail();
        }

        [Test, Repeat(2)]
        public void TestUnknownCommand()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                for (int i = 0; i < 10; i++)
                {
                    string commandName = Guid.NewGuid().ToString().Substring(0, 3);
                    string command = commandName + " " + DateTime.Now;
                    socket.SendTo(m_Encoding.GetBytes(command + Environment.NewLine), serverAddress);
                    string line = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                    Console.WriteLine(line);
                    Assert.AreEqual(string.Format(TestSession.UnknownCommandMessageFormat, commandName), line);
                }
            }
        }

        [Test, Repeat(3)]
        public void TestCustomCommandName()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                string param = Guid.NewGuid().ToString();
                string command = "325 " + param + Environment.NewLine;
                socket.SendTo(m_Encoding.GetBytes(command), serverAddress);
                string echoMessage = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                Console.WriteLine("C:" + echoMessage);
                Assert.AreEqual(string.Format(SuperSocket.Test.Command.NUM.ReplyFormat, param), echoMessage);
            }
        }

        [Test, Repeat(3)]
        public void TestCommandParser()
        {
            var server = new TestServer(new TestCommandParser());

            IBootstrap bootstrap = new DefaultBootstrap();
            bootstrap.Initialize(m_RootConfig, new IAppServer[] { server }, new IServerConfig[] { m_Config });

            try
            {
                server.Start();

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

                using (Socket socket = CreateClientSocket())
                {
                    string command = string.Format("Hello World ({0})!", Guid.NewGuid().ToString());
                    socket.SendTo(m_Encoding.GetBytes("ECHO:" + command + Environment.NewLine), serverAddress);
                    string echoMessage = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                    Assert.AreEqual(command, echoMessage);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (server.IsRunning)
                    server.Stop();
            }
        }

        //[Test, Repeat(3)]
        //public void TestCommandParameterParser()
        //{
        //    var server = new TestServer(new TestCommandParser(), new TestCommandParameterParser());
        //    server.Setup(m_Config);

        //    try
        //    {
        //        server.Start();

        //        EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

        //        using (Socket socket = CreateClientSocket())
        //        {
        //            string[] arrParam = new string[] { "A1", "A2", "A4", "B2", "A6", "E5" };
        //            socket.SendTo(m_Encoding.GetBytes("PARA:" + string.Join(",", arrParam) + Environment.NewLine), serverAddress);

        //            List<string> received = new List<string>();

        //            foreach (var p in arrParam)
        //            {
        //                string r = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
        //                Console.WriteLine("C: " + r);
        //                received.Add(r);
        //            }

        //            Assert.AreEqual(arrParam, received);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //    finally
        //    {
        //        if (server.IsRunning)
        //            server.Stop();
        //    }
        //}

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
                Port = defaultConfig.Port
            };

            IBootstrap bootstrap = new DefaultBootstrap();
            bootstrap.Initialize(m_RootConfig, new IAppServer[] { server }, new IServerConfig[] { config });

            List<Socket> sockets = new List<Socket>();

            try
            {
                server.Start();

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

                for (int i = 0; i < maxConnectionNumber; i++)
                {
                    Socket socket = CreateClientSocket();
                    socket.SendTo(m_Encoding.GetBytes(Guid.NewGuid().ToString() + Environment.NewLine), serverAddress);
                    Console.WriteLine("C: " + m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray()));
                    sockets.Add(socket);
                }

                using (Socket trySocket = CreateClientSocket())
                {
                    trySocket.SendTo(m_Encoding.GetBytes(Guid.NewGuid().ToString() + Environment.NewLine), serverAddress);
                    Thread thread = new Thread(new ThreadStart(() =>
                        {
                            Console.WriteLine("C: " + m_Encoding.GetString(ReceiveMessage(trySocket, serverAddress).ToArray()));
                        }));
                    thread.Start();
                    if (thread.Join(500))
                    {
                        //Assert.Fail("Current connection number: {0}, max connectionnumber: {1}", maxConnectionNumber + 1, maxConnectionNumber);
                        return false;
                    }
                    else
                    {
                        thread.Abort();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                return false;
            }
            finally
            {
                server.Stop();
            }
        }

        [Test]
        public void TestMaxConnectionNumber()
        {
            Assert.IsTrue(TestMaxConnectionNumber(1));
            Assert.IsTrue(TestMaxConnectionNumber(2));
            Assert.IsTrue(TestMaxConnectionNumber(5));
            Assert.IsTrue(TestMaxConnectionNumber(15));
        }

        [Test, Repeat(5)]
        public void TestClearTimeoutSession()
        {
            StartServer();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

            using (Socket socket = CreateClientSocket())
            {
                string param = Guid.NewGuid().ToString();
                string command = "325 " + param + Environment.NewLine;
                socket.SendTo(m_Encoding.GetBytes(command), serverAddress);
                string echoMessage = m_Encoding.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                Console.WriteLine("C:" + echoMessage);
            }

            Assert.AreEqual(1, m_Server.SessionCount);
            Thread.Sleep(2000);
            Assert.AreEqual(1, m_Server.SessionCount);
            Thread.Sleep(5000);
            Assert.AreEqual(0, m_Server.SessionCount);
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
