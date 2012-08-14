using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    [TestFixture]
    public class TestSocketServer
    {
        [Test]
        public void TestECHO()
        {
            IServerConfig config = new ServerConfig
            {
                Name = "My Custom Server",
                Ip = "Any",
                Port = 100,
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = 1
            };

            var rootConfig = new RootConfig();

            YourServer server = new YourServer();
            server.Setup(rootConfig, config, logFactory: new ConsoleLogFactory());

            server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), config.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, Encoding.Default, true))
                using (StreamWriter writer = new StreamWriter(socketStream, Encoding.Default, 1024 * 8))
                {
                    //ignore welcome message
                    reader.ReadLine();

                    string command = "CMD:ECHO ";
                    string[] parameters = new string[] { "Kerry", "Jiang", "China", "Shanghai" };
                    string parameter = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Join(" ", parameters)));
                    writer.WriteLine(command + parameter);
                    writer.Flush();

                    foreach (var p in parameters)
                    {
                        string param = reader.ReadLine();
                        Console.WriteLine(param);
                        Assert.AreEqual(p, param);
                    }
                }
            }

            server.Stop();
        }
    }
}
