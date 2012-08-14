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
                    null, new TerminatorRequestFilterFactory("##", m_Encoding), new ConsoleLogFactory(), null, null);

                Assert.IsTrue(setupResult);
                Assert.IsTrue(appServer.Start());

                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2012);

                using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(serverAddress);
                    Stream socketStream = new NetworkStream(socket);
                    using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                    using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                    {
                        var welcomeString = reader.ReadLine();
                        Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, appServer.Name), welcomeString);

                        char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                        Random rd = new Random(1);

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < 100; i++)
                        {
                            sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                            string command = sb.ToString();
                            writer.Write("ECHO " + command);
                            writer.Write("##");
                            writer.Flush();
                            string echoMessage = reader.ReadLine();
                            Console.WriteLine("C:" + echoMessage);
                            Assert.AreEqual(command, echoMessage);
                        }
                    }
                }
                
            }
        }
    }
}
