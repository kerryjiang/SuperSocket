using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
#if !IL20
using mscoree;//Add the following as a COM reference - C:\WINDOWS\Microsoft.NET\Framework\vXXXXXX\mscoree.tlb
#endif
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketEngine;


namespace SuperSocket.Test
{
    [TestFixture]
    public class AppDomainSocketServerTest : TcpSocketServerTest
    {
        protected override IWorkItem CreateAppServer<T>(IRootConfig rootConfig, IServerConfig serverConfig)
        {
            var appServer = new AppDomainAppServer(typeof(T));

            var config = new ConfigurationSource();
            rootConfig.CopyPropertiesTo(config);

            appServer.Setup(new AppDomainBootstrap(config), serverConfig, new ProviderFactoryInfo[]
            {
                new ProviderFactoryInfo(ProviderKey.SocketServerFactory, ProviderKey.SocketServerFactory.Name, typeof(SocketServerFactory)),
                new ProviderFactoryInfo(ProviderKey.LogFactory, ProviderKey.LogFactory.Name, typeof(ConsoleLogFactory))
            });

            return appServer;
        }

        [Test]
        public void TestAppDomain()
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

                    writer.WriteLine("DOMAIN");
                    writer.Flush();

                    var line = reader.ReadLine();

                    Assert.AreEqual(m_Config.Name, line);
                }
            }
        }

#if !IL20
        [Test]
        public void TestAppDomainLifetime()
        {
            StartServer();

            Assert.IsTrue(DetectAppDomain(m_Config.Name));

            StopServer();

            Assert.IsFalse(DetectAppDomain(m_Config.Name));
        }

        private bool DetectAppDomain(string domainName)
        {
            IntPtr enumHandle = IntPtr.Zero;

            ICorRuntimeHost host = new CorRuntimeHost();

            try
            {
                host.EnumDomains(out enumHandle);

                object domain = null;

                while (true)
                {

                    host.NextDomain(enumHandle, out domain);

                    if (domain == null)
                        break;

                    AppDomain appDomain = (AppDomain)domain;

                    if (appDomain.FriendlyName.Equals(domainName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            finally
            {
                host.CloseEnum(enumHandle);
                Marshal.ReleaseComObject(host);
            }
        }
#endif
    }
}
