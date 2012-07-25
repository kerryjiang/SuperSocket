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

        private IConfigurationSource SetupBootstrap(string configFile)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = Path.Combine(@"Config", configFile);

            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var configSource = config.GetSection("socketServer") as IConfigurationSource;

            m_BootStrap = BootstrapFactory.CreateBootstrap(configSource);

            Assert.IsTrue(m_BootStrap.Initialize());

            var result = m_BootStrap.Start();

            Assert.AreEqual(StartResult.Success, result);

            return configSource;
        }

        [Test]
        public void TestBasicConfig()
        {
            var configSource = SetupBootstrap("Basic.config");

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), configSource.Servers.FirstOrDefault().Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
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
                    using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
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
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
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
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
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
    }
}
