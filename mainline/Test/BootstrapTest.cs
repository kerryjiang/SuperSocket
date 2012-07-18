using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketEngine;
using System.Configuration;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using System.Net;
using System.Net.Sockets;
using System.IO;

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
            fileMap.ExeConfigFilename = @"Config\" + configFile;

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
    }
}
