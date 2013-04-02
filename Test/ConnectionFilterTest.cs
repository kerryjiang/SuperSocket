using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;
using SuperSocket.Test.ConnectionFilter;

namespace SuperSocket.Test
{
    [TestFixture]
    public class ConnectionFilterTest : BootstrapTestBase
    {
        private AutoResetEvent m_NewSessionConnectedEvent = new AutoResetEvent(false);

        void m_Server_NewSessionConnected(TestSession obj)
        {
            m_NewSessionConnectedEvent.Set();
        }

        [Test]
        public void TestInitialize()
        {
            StartBootstrap("ConnectionFilter.config");
            var appServer = BootStrap.AppServers.FirstOrDefault() as TestServer;
            
            var connectionFilter = appServer.ConnectionFilters.FirstOrDefault() as TestConnectionFilter;

            Assert.AreEqual("TestFilter", connectionFilter.Name);
            Assert.AreEqual(appServer, connectionFilter.AppServer);
        }

        [Test]
        public void TestAllow()
        {
            var configSource = StartBootstrap("ConnectionFilter.config");
            var serverConfig = configSource.Servers.FirstOrDefault();
            var appServer = BootStrap.AppServers.FirstOrDefault() as TestServer;
            appServer.NewSessionConnected += m_Server_NewSessionConnected;

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                TestConnectionFilter.Allow = true;
                int oldCount = TestConnectionFilter.ConnectedCount;

                var signal = new ManualResetEventSlim(false);

                ThreadPool.QueueUserWorkItem((o) =>
                    {
                        var s = o as Socket;
                        s.Connect(serverAddress);
                        signal.Set();
                    }, socket);

                Assert.IsTrue(signal.Wait(2000));
                Thread.Sleep(100);
                Assert.AreEqual(oldCount + 1, TestConnectionFilter.ConnectedCount);

                if (!m_NewSessionConnectedEvent.WaitOne(1000))
                    Assert.Fail("New session hasnot been accept on time!");
            }
        }

        [Test]
        public void TestDisallow()
        {
            var configSource = StartBootstrap("ConnectionFilter.config");
            var serverConfig = configSource.Servers.FirstOrDefault();
            var appServer = BootStrap.AppServers.FirstOrDefault() as TestServer;
            appServer.NewSessionConnected += m_Server_NewSessionConnected;

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                TestConnectionFilter.Allow = false;

                var signal = new ManualResetEventSlim(false);

                int oldCount = TestConnectionFilter.ConnectedCount;

                Task.Factory.StartNew((o) =>
                    {
                        var s = o as Socket;
                        s.Connect(serverAddress);
                        signal.Set();
                    }, socket);

                Assert.IsTrue(signal.Wait(2000));
                Thread.Sleep(100);
                Assert.AreEqual(oldCount, TestConnectionFilter.ConnectedCount);

                if (m_NewSessionConnectedEvent.WaitOne(1000))
                    Assert.Fail("The connection filter doesn't work as requirement!");
            }
        }
    }
}
