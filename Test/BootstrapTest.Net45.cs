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
    public partial class BootstrapTest : BootstrapTestBase
    {
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
            var configSource = StartBootstrap("DefaultCultureA.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            ValidateCulture(serverConfig.Port, "zh-CN");
        }

        private void TestDefaultCultureB()
        {
            var configSource = StartBootstrap("DefaultCultureB.config");

            var serverConfig = configSource.Servers.FirstOrDefault();

            ValidateCulture(serverConfig.Port, "zh-CN");
        }

        private void TestDefaultCultureC()
        {
            var configSource = StartBootstrap("DefaultCultureC.config");

            var serverConfigA = configSource.Servers.FirstOrDefault(s => s.Name == "TestServerA");
            ValidateCulture(serverConfigA.Port, "zh-TW");

            var serverConfigB = configSource.Servers.FirstOrDefault(s => s.Name == "TestServerB");
            ValidateCulture(serverConfigB.Port, "zh-CN");
        }
    }
}
