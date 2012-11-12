using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SuperSocket.Test
{
    [TestFixture]
    public class ProtocolServerTest
    {
        private readonly Encoding m_Encoding = new ASCIIEncoding();

        [Test]
        public void TestTerminatorRequestFilter()
        {
            var appServer = new TestServer();

            using (appServer as IDisposable)
            {

                var setupResult = appServer.Setup("127.0.0.1", 2012,
                    null, new TerminatorReceiveFilterFactory("##", m_Encoding), new ConsoleLogFactory(), null, null);

                Assert.IsTrue(setupResult);
                Assert.IsTrue(appServer.Start());

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2012);

                using (var socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(serverAddress);
                    var socketStream = new NetworkStream(socket);
                    using (var reader = new StreamReader(socketStream, m_Encoding, true))
                    using (var writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                    {
                        var welcomeString = reader.ReadLine();
                        Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, appServer.Name), welcomeString);

                        var chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                        var rd = new Random(1);

                        var sb = new StringBuilder();

                        for (var i = 0; i < 100; i++)
                        {
                            sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                            var command = sb.ToString();
                            writer.Write("ECHO " + command);
                            writer.Write("##");
                            writer.Flush();
                            var echoMessage = reader.ReadLine();
                            Console.WriteLine("C:" + echoMessage);
                            Assert.AreEqual(command, echoMessage);
                        }
                    }
                }
                
            }
        }


        [Test]
        public void TestTerminatorRequestFilterA()
        {
            var appServer = new TestServer();

            using (appServer as IDisposable)
            {

                var setupResult = appServer.Setup("127.0.0.1", 2012,
                    null, new TerminatorReceiveFilterFactory("##", m_Encoding), new ConsoleLogFactory(), null, null);

                Assert.IsTrue(setupResult);
                Assert.IsTrue(appServer.Start());

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2012);

                using (var socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(serverAddress);
                    var socketStream = new NetworkStream(socket);
                    using (var reader = new StreamReader(socketStream, m_Encoding, true))
                    using (var writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                    {
                        var welcomeString = reader.ReadLine();
                        Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, appServer.Name), welcomeString);

                        var actions = new Action<ConsoleWriter, string>[]
                            {
                                (w, r) => SendRequestA(w, r),
                                (w, r) => SendRequestB(w, r),
                                (w, r) => SendRequestC(w, r)
                            };

                        var rd = new Random();

                        for(var i = 0; i < 50; i++)
                        {
                            var command = Guid.NewGuid().ToString();

                            var act = actions[rd.Next(0, 100) % actions.Length];

                            act(writer, command);

                            var echoMessage = reader.ReadLine();
                            Console.WriteLine("C:" + echoMessage);
                            Assert.AreEqual(command, echoMessage);
                        }
                    }
                }
            }
        }

        private void SendRequestA(ConsoleWriter writer, string line)
        {
            writer.Write("ECHO " + line);
            writer.Flush();

            Thread.Sleep(100);

            writer.Write("##");
            writer.Flush();
        }

        private void SendRequestB(ConsoleWriter writer, string line)
        {
            writer.Write("ECHO " + line);
            writer.Flush();

            Thread.Sleep(100);

            writer.Write("#");
            writer.Flush();

            Thread.Sleep(100);

            writer.Write("#");
            writer.Flush();
        }

        private void SendRequestC(ConsoleWriter writer, string line)
        {
            writer.Write("ECHO " + line.Substring(0, line.Length - 1));
            writer.Flush();

            Thread.Sleep(100);
            writer.Write(line.Substring(line.Length - 1) + "#");
            writer.Flush();

            Thread.Sleep(100);

            writer.Write("#");
            writer.Flush();
        }
    }
}
