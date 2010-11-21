using System;
using System.Collections.Generic;
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
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            m_Server = new TestServer();
            m_Server.Setup(m_Config, SocketServerFactory.Instance);
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
                    socket.SendTo(Encoding.UTF8.GetBytes("ECHO " + command + "\r\n"), serverAddress);
                    Console.WriteLine("Client sent:" + command);
                    string echoMessage = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                    Console.WriteLine("C:" + echoMessage);
                    Assert.AreEqual(command, echoMessage);
                }
            }
        }

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
                    socket.SendTo(Encoding.UTF8.GetBytes("ECHO " + command + "\r\n"), serverAddress);
                    string echoMessage = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
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

            List<ManualResetEvent> events = new List<ManualResetEvent>();
            Semaphore semaphore = new Semaphore(concurrencyCount, concurrencyCount * 2);

            for (var i = 0; i < concurrencyCount - 1; i++)
            {
                var resetEvent = new ManualResetEvent(false);
                events.Add(resetEvent);
            }

            System.Threading.Tasks.Parallel.For(0, concurrencyCount - 1, i =>
                {
                    if (RunEchoMessage())
                        events[i].Set();
                    semaphore.Release();
                });

            semaphore.WaitOne();

            if (!WaitHandle.WaitAll(events.ToArray(), 1000))
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
                    socket.SendTo(Encoding.UTF8.GetBytes(command + "\r\n"), serverAddress);
                    string line = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
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
                socket.SendTo(Encoding.UTF8.GetBytes(command), serverAddress);
                string echoMessage = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
                Console.WriteLine("C:" + echoMessage);
                Assert.AreEqual(string.Format(SuperSocket.Test.Command.NUM.ReplyFormat, param), echoMessage);
            }
        }

        [Test, Repeat(3)]
        public void TestCommandParser()
        {
            var server = new TestServer(new TestCommandParser());
            server.Setup(m_Config, SocketServerFactory.Instance);

            try
            {
                server.Start();

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

                using (Socket socket = CreateClientSocket())
                {
                    string command = string.Format("Hello World ({0})!", Guid.NewGuid().ToString());
                    socket.SendTo(Encoding.UTF8.GetBytes("ECHO:" + command + Environment.NewLine), serverAddress);
                    string echoMessage = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
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
        //            socket.SendTo(Encoding.UTF8.GetBytes("PARA:" + string.Join(",", arrParam) + Environment.NewLine), serverAddress);

        //            List<string> received = new List<string>();

        //            foreach (var p in arrParam)
        //            {
        //                string r = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
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

            server.Setup(config, SocketServerFactory.Instance);

            List<Socket> sockets = new List<Socket>();

            try
            {
                server.Start();

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

                for (int i = 0; i < maxConnectionNumber; i++)
                {
                    Socket socket = CreateClientSocket();
                    socket.SendTo(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Environment.NewLine), serverAddress);
                    Console.WriteLine("C: " + Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray()));
                    sockets.Add(socket);
                }

                using (Socket trySocket = CreateClientSocket())
                {
                    trySocket.SendTo(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + Environment.NewLine), serverAddress);
                    Thread thread = new Thread(new ThreadStart(() =>
                        {
                            Console.WriteLine("C: " + Encoding.UTF8.GetString(ReceiveMessage(trySocket, serverAddress).ToArray()));
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
                socket.SendTo(Encoding.UTF8.GetBytes(command), serverAddress);
                string echoMessage = Encoding.UTF8.GetString(ReceiveMessage(socket, serverAddress).ToArray());
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
