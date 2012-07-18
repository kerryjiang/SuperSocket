using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using SuperSocket.Common.Logging;
using SuperSocket.SocketEngine;
using SuperSocket.Test.ConnectionFilter;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Test
{
    [TestFixture]
    public class ConnectionFilterTest
    {
        private TestServer m_Server;

        private IServerConfig m_ServerConfig;

        private Encoding m_Encoding;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_Encoding = new UTF8Encoding();
            m_ServerConfig = new ServerConfig
                {
                    Ip = "Any",
                    LogCommand = false,
                    MaxConnectionNumber = 1,
                    Mode = SocketMode.Tcp,
                    Name = "TestServer",
                    Port = 2012,
                    ClearIdleSession = false
                };

            m_Server = new TestServer();
            m_Server.NewSessionConnected += new Action<TestSession>(m_Server_NewSessionConnected);
            m_Server.Setup(new RootConfig(), m_ServerConfig, SocketServerFactory.Instance, null, new ConsoleLogFactory(), new IConnectionFilter[] { new TestConnectionFilter() }, null);
        }


        private AutoResetEvent m_NewSessionConnectedEvent = new AutoResetEvent(false);

        void m_Server_NewSessionConnected(TestSession obj)
        {
            m_NewSessionConnectedEvent.Set();
        }

        [TearDown]
        public void StopServer()
        {
            if (m_Server.IsRunning)
                m_Server.Stop();
        }

        [Test]
        public void TestAllow()
        {
            if (!m_Server.IsRunning)
                m_Server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_ServerConfig.Port);

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
            if (!m_Server.IsRunning)
                m_Server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_ServerConfig.Port);

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
