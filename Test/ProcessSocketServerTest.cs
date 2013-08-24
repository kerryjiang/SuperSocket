using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SuperSocket.Test
{
    [TestFixture]
    public class ProcessSocketServerTest : SocketServerTest
    {
        protected override string DefaultServerConfig
        {
            get
            {
                return "ProcessTestServer.config";
            }
        }

        [Test]
        public void TestProcess()
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

                    writer.WriteLine("PROC");
                    writer.Flush();

                    var line = reader.ReadLine();

                    var pars = line.Split(',');
                    var appDomainName = pars[0];
                    var appDomainRoot = pars[1];

                    Assert.AreEqual("SuperSocket.Agent.exe", appDomainName);
                    Assert.AreEqual(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppRoot", serverConfig.Name), appDomainRoot);
                }
            }
        }
    }
}
