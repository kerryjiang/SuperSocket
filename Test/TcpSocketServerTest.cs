using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using SuperSocket.Test.Command;

namespace SuperSocket.Test
{
    [TestFixture]
    public class TcpSocketServerTest : SocketServerTest
    {
        protected override string DefaultServerConfig
        {
            get
            {
                return "TestServer.config";
            }
        }

        private IBootstrap m_ActiveServerBootstrap;

        protected override void OnBootstrapCleared()
        {
            if (m_ActiveServerBootstrap != null)
            {
                m_ActiveServerBootstrap.Stop();
                m_ActiveServerBootstrap = null;
                AppDomain.CurrentDomain.SetData("Bootstrap", null);
            }
        }

        [Test]
        public void TestActiveConnect()
        {
            var configSource = StartBootstrap(DefaultServerConfig);

            IBootstrap activeServerBootstrap;
            var activeTargetServerConfig = CreateBootstrap("ActiveConnectServer.config", out activeServerBootstrap);

            Assert.IsTrue(activeServerBootstrap.Initialize());

            var serverConfig = activeTargetServerConfig.Servers.FirstOrDefault();
            var serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            m_ActiveServerBootstrap = AppDomain.CurrentDomain.GetData("Bootstrap") as IBootstrap;

            Assert.AreEqual(StartResult.Success, m_ActiveServerBootstrap.Start());

            var appServer = BootStrap.AppServers.FirstOrDefault() as TestServer;

            var task = appServer.ActiveConnectRemote(serverAddress);

            if (!task.Wait(5000))
            {
                Assert.Fail("Active connect the server timeout");
            }

            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);

            Assert.AreEqual(true, task.Result.Result);
            Assert.IsNotNull(task.Result.Session);

            Assert.AreEqual(1, m_ActiveServerBootstrap.AppServers.FirstOrDefault().SessionCount);

            var session = task.Result.Session as TestSession;

            var rd = new Random();
            var a = rd.Next(1, 1000);
            var b = rd.Next(1, 1000);
            session.Send("ADDR " + a + " " + b);
            Thread.Sleep(500);

            Assert.AreEqual((a + b).ToString(), RESU.Result);
        }
    }
}
