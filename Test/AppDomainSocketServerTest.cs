using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketEngine;


namespace SuperSocket.Test
{
    [TestFixture]
    public class AppDomainSocketServerTest : SocketServerTest
    {
        protected override string DefaultServerConfig
        {
            get
            {
                return "AppDomainTestServer.config";
            }
        }

        [Test]
        public void TestAppDomain()
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

                    writer.WriteLine("DOMAIN");
                    writer.Flush();

                    var line = reader.ReadLine();

                    var pars = line.Split(',');
                    var appDomainName = pars[0];
                    var appDomainRoot = pars[1];

                    Assert.AreEqual(serverConfig.Name, appDomainName);
                    Assert.AreEqual(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppRoot"), serverConfig.Name), appDomainRoot);
                }
            }
        }
    }
}
