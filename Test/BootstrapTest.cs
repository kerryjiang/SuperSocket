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
    public class BootstrapTest
    {
        private IBootstrap m_BootStrap;

        private Encoding m_Encoding = new UTF8Encoding();

        [TearDown]
        public void ClearBootstrap()
        {
            if(m_BootStrap != null)
            {
                m_BootStrap.Stop();
                m_BootStrap = null;
            }
        }

        private IConfigurationSource CreateBootstrap(string configFile)
        {
            var fileMap = new ExeConfigurationFileMap();
            var filePath = Path.Combine(@"Config", configFile);
            fileMap.ExeConfigFilename = filePath;
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var configSource = config.GetSection("superSocket") as IConfigurationSource;

            m_BootStrap = BootstrapFactory.CreateBootstrap(configSource);

            return configSource;
        }

        private IConfigurationSource SetupBootstrap(string configFile)
        {
            var configSource = CreateBootstrap(configFile);

            Assert.IsTrue(m_BootStrap.Initialize());

            var result = m_BootStrap.Start();

            Assert.AreEqual(StartResult.Success, result);

            return configSource;
        }

        [Test]
        public void TestBasicConfig()
        {
            TestBasicConfig("Basic.config");
        }

        private void TestBasicConfig(string configFile)
        {
            var configSource = SetupBootstrap(configFile);

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
            var configSource = SetupBootstrap("Listeners.config");

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
            var configSource = SetupBootstrap("AppDomain.config");

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
                    Assert.AreEqual(serverConfig.Name, reader.ReadLine());
                }
            }
        }

        [Test]
        public void TestDLRConfig()
        {
            var configSource = SetupBootstrap("DLR.config");

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

            m_BootStrap.Initialize(endPointReplacement);

            var appServer = m_BootStrap.AppServers.OfType<IAppServer>().FirstOrDefault();

            Assert.AreEqual(1, appServer.Listeners.Length);

            Assert.AreEqual(3012, appServer.Listeners[0].EndPoint.Port);

            CreateBootstrap("Listeners.config");

            endPointReplacement = new Dictionary<string, IPEndPoint>(StringComparer.OrdinalIgnoreCase);
            endPointReplacement.Add("TestServer_2012", new IPEndPoint(IPAddress.Any, 3012));
            endPointReplacement.Add("TestServer_2013", new IPEndPoint(IPAddress.Any, 3013));

            m_BootStrap.Initialize(endPointReplacement);

            appServer = m_BootStrap.AppServers.OfType<IAppServer>().FirstOrDefault();

            Assert.AreEqual(2, appServer.Listeners.Length);

            Assert.AreEqual(3012, appServer.Listeners[0].EndPoint.Port);
            Assert.AreEqual(3013, appServer.Listeners[1].EndPoint.Port);
        }

        [Test]
        public void TestDefaultCulture()
        {
            TestDefaultCultureA();
            ClearBootstrap();

            TestDefaultCultureB();
            ClearBootstrap();

            TestDefaultCultureC();
            ClearBootstrap();
        }

        private void ValidateCulture(int port, string culture)
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (ConsoleWriter writer = new ConsoleWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();
                    writer.WriteLine("CULT");
                    writer.Flush();
                    var realCult = reader.ReadLine();
                    Console.WriteLine(realCult);
                    Assert.AreEqual(culture, realCult);
                }
            }
        }

        private void TestDefaultCultureA()
        {
            var configSource = SetupBootstrap("DefaultCultureA.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            ValidateCulture(serverConfig.Port, "zh-CN");
        }

        private void TestDefaultCultureB()
        {
            var configSource = SetupBootstrap("DefaultCultureB.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            ValidateCulture(serverConfig.Port, "zh-CN");
        }

        private void TestDefaultCultureC()
        {
            var configSource = SetupBootstrap("DefaultCultureC.config");

            var serverConfigA = configSource.Servers.FirstOrDefault(s => s.Name == "TestServerA");
            ValidateCulture(serverConfigA.Port, "zh-TW");

            var serverConfigB = configSource.Servers.FirstOrDefault(s => s.Name == "TestServerB");
            ValidateCulture(serverConfigB.Port, "zh-CN");
        }
    }
}
