using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;

namespace SuperSocket.Test
{
    [TestFixture]
    public partial class BootstrapTest : BootstrapTestBase
    {
        private Encoding m_Encoding = new UTF8Encoding();

        [Test]
        public void TestBasicConfig()
        {
            TestBasicConfig("Basic.config");
        }

        private void TestBasicConfig(string configFile)
        {
            var configSource = StartBootstrap(configFile);

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), configSource.Servers.FirstOrDefault().Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    for (var i = 0; i < 10; i++)
                    {
                        var message = Guid.NewGuid().ToString();
                        writer.WriteLine("ECHO {0}", message);
                        writer.Flush();
                        Assert.AreEqual(message, reader.ReadLine());
                    }
                }
            }
        }

        [Test]
        public void TestServerTypeConfig()
        {
            TestBasicConfig("ServerType.config");
        }


        [Test]
        public void TestListenersConfig()
        {
            var configSource = StartBootstrap("Listeners.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            foreach (var listener in serverConfig.Listeners)
            {
                EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listener.Port);

                using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(serverAddress);
                    Stream socketStream = new NetworkStream(socket);
                    using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                    using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                    {
                        reader.ReadLine();

                        for (var i = 0; i < 5; i++)
                        {
                            var message = Guid.NewGuid().ToString();
                            writer.WriteLine("ECHO {0}", message);
                            writer.Flush();
                            Assert.AreEqual(message, reader.ReadLine());
                        }
                    }
                }
            }
        }

        [Test]
        public void TestAppDomainConfig()
        {
            var configSource = StartBootstrap("AppDomain.config");

            var serverConfig = configSource.Servers.FirstOrDefault();
            
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    writer.WriteLine("DOMAIN");
                    writer.Flush();

                    var line = reader.ReadLine();
                    var pars = line.Split(',');
                    var appDomainName = pars[0];

                    Assert.AreEqual(serverConfig.Name, appDomainName);
                }
            }
        }

        [Test]
        public void TestProcessIsolationConfig()
        {
            var configSource = StartBootstrap("ProcessIsolation.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    writer.WriteLine("PROC");
                    writer.Flush();

                    var line = reader.ReadLine();

                    var pars = line.Split(',');
                    var appDomainName = pars[0];

                    Assert.AreEqual("SuperSocket.Agent.exe", appDomainName);
                }
            }
        }

        [Test]
        public void TestDLRConfig()
        {
            var configSource = StartBootstrap("DLR.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    Random rd = new Random();

                    for (int i = 0; i < 5; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "ADD", x, y);
                        Console.WriteLine(command);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Console.WriteLine(line);
                        Assert.AreEqual(x + y, int.Parse(line));
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "MULT", x, y);
                        Console.WriteLine(command);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Console.WriteLine(line);
                        Assert.AreEqual(x * y, int.Parse(line));
                    }
                }
            }
        }


        [Test]
        public void TestCustomChildConfig()
        {
            SetupBootstrap("ChildConfigA.config");
            Assert.AreEqual(168, ChildConfigTestServer.ChildConfigValue);
            ClearBootstrap();

            SetupBootstrap("ChildConfigB.config");
            Assert.AreEqual(192, ChildConfigTestServer.ChildConfigValue);
            ClearBootstrap();

            SetupBootstrap("ChildConfigC.config");
            Assert.AreEqual(200, ChildConfigTestServer.ChildConfigValue);
            ClearBootstrap();

            SetupBootstrap("ChildConfigD.config");
            Assert.AreEqual(8848, ChildrenConfigTestServer.ChildConfigGlobalValue);
            Assert.AreEqual(10 + 20 + 30, ChildrenConfigTestServer.ChildConfigValueSum);
            Assert.AreEqual(10 * 20 * 30, ChildrenConfigTestServer.ChildConfigValueMultiplyProduct);
        }

        [Test]
        public void TestListenEndPointReplacement()
        {
            CreateBootstrap("Basic.config");

            var endPointReplacement = new Dictionary<string, IPEndPoint>(StringComparer.OrdinalIgnoreCase);
            endPointReplacement.Add("TestServer_2012", new IPEndPoint(IPAddress.Any, 3012));

            BootStrap.Initialize(endPointReplacement);

            var appServer = BootStrap.AppServers.OfType<IAppServer>().FirstOrDefault();

            Assert.AreEqual(1, appServer.Listeners.Length);

            Assert.AreEqual(3012, appServer.Listeners[0].EndPoint.Port);

            CreateBootstrap("Listeners.config");

            endPointReplacement = new Dictionary<string, IPEndPoint>(StringComparer.OrdinalIgnoreCase);
            endPointReplacement.Add("TestServer_2012", new IPEndPoint(IPAddress.Any, 3012));
            endPointReplacement.Add("TestServer_2013", new IPEndPoint(IPAddress.Any, 3013));

            BootStrap.Initialize(endPointReplacement);

            appServer = BootStrap.AppServers.OfType<IAppServer>().FirstOrDefault();

            Assert.AreEqual(2, appServer.Listeners.Length);

            Assert.AreEqual(3012, appServer.Listeners[0].EndPoint.Port);
            Assert.AreEqual(3013, appServer.Listeners[1].EndPoint.Port);
        }
    }
}
