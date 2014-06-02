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
using System.Net.Sockets;
using System.IO;
using System.Collections.Specialized;

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
            StartBootstrap(DefaultServerConfig);

            IBootstrap activeServerBootstrap;
            var activeTargetServerConfig = CreateBootstrap("ActiveConnectServer.config", out activeServerBootstrap);

            Assert.IsTrue(activeServerBootstrap.Initialize());

            var serverConfig = activeTargetServerConfig.Servers.FirstOrDefault();
            var serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            m_ActiveServerBootstrap = activeServerBootstrap;

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

            var remoteServer = m_ActiveServerBootstrap.AppServers.FirstOrDefault() as TestServer;

            Thread.Sleep(100);

            Assert.AreEqual(1, appServer.SessionCount);
            Assert.AreEqual(1, remoteServer.SessionCount);

            var session = task.Result.Session as TestSession;

            var rd = new Random();
            var a = rd.Next(1, 1000);
            var b = rd.Next(1, 1000);
            session.Send("ADDR " + a + " " + b);
            Thread.Sleep(500);

            Assert.AreEqual((a + b).ToString(), RESU.Result);

            var resetEvent = new AutoResetEvent(false);

            //Reconnect
            session.SocketSession.Closed += (s, c) =>
                {
                    resetEvent.WaitOne();
                    Thread.Sleep(5000);
                    var t = appServer.ActiveConnectRemote(serverAddress);
                    t.ContinueWith((x) =>
                        {
                            if (x.Exception != null)
                                Console.WriteLine(x.Exception.InnerException.Message);

                            resetEvent.Set();
                        });
                };

            foreach (var s in remoteServer.GetAllSessions())
            {
                s.Close();
            }

            Thread.Sleep(500);

            Assert.AreEqual(0, appServer.SessionCount);
            Assert.AreEqual(0, remoteServer.SessionCount);

            resetEvent.Set();
            Thread.Sleep(500);
            resetEvent.WaitOne();

            Thread.Sleep(500);

            Assert.AreEqual(1, appServer.SessionCount);
            Assert.AreEqual(1, remoteServer.SessionCount);

        }

        [Test]
        public void TestAddServerInRuntime()
        {
            var configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            var filePath = Path.Combine(configDir, DefaultServerConfig);
            var bakFilePath = Path.Combine(configDir, DefaultServerConfig + ".tmp");

            // backup the configuration file for restore
            File.Copy(filePath, bakFilePath, true);

            try
            {
                TestAddServerInRuntimeImplement();
            }
            finally
            {
                // restore the configiration file
                File.Copy(bakFilePath, filePath, true);
            }
        }

        void TestAddServerInRuntimeImplement()
        {
            StartBootstrap(DefaultServerConfig);
            var bootstrap = BootStrap as IDynamicBootstrap;

            var options = new NameValueCollection();
            options["testAtt1"] = "1";
            options["testAtt2"] = "2";

            Assert.IsTrue(bootstrap.AddAndStart(new ServerConfig
            {
                Name = "TestDynamicServer",
                ServerType = "SuperSocket.Test.TestServer, SuperSocket.Test",
                Port = 2013,
                Options = options
            }));

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2013);

            using (Socket socket = CreateClientSocket())
            {
                try
                {
                    socket.Connect(serverAddress);
                }
                catch
                {
                    Assert.Fail("Failed to connect to the dynamic created server.");
                }

                Stream socketStream = GetSocketStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();
                    Assert.AreEqual(string.Format(TestSession.WelcomeMessageFormat, "TestDynamicServer"), welcomeString);
                    var line = Guid.NewGuid().ToString();
                    writer.WriteLine("ECHO " + line);
                    writer.Flush();

                    Assert.AreEqual(line, reader.ReadLine());
                }
            }

            BootStrap.GetServerByName("TestDynamicServer").Stop();
            bootstrap.Remove("TestDynamicServer");
        }
    }
}
