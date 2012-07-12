using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common.Logging;
using SuperSocket.Dlr;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;

namespace SuperSocket.Test
{
    [TestFixture]
    public class DLRSocketServerTest
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
                    MaxConnectionNumber = 100,
                    Mode = SocketMode.Tcp,
                    Name = "DLR Socket Server",
                    Port = 2012,
                    ClearIdleSession = true,
                    ClearIdleSessionInterval = 1,
                    IdleSessionTimeOut = 5
                };

            m_Server = new TestServer();
            var scriptRuntime = new ScriptRuntime(new ScriptRuntimeSetup
                {
                    LanguageSetups =
                    {
                        new LanguageSetup("IronPython.Runtime.PythonContext, IronPython", "IronPython", "IronPython;Python;py".Split(';'), new string[] { ".py" })
                    }
                });
            m_Server.Setup(new RootConfig(), m_ServerConfig, SocketServerFactory.Instance, null, new ConsoleLogFactory(), null, new ICommandLoader[] { new DynamicCommandLoader(scriptRuntime) });
        }

        [TearDown]
        public void StopServer()
        {
            if (m_Server.IsRunning)
                m_Server.Stop();
        }

        [Test]
        public void TestCommands()
        {
            if (!m_Server.IsRunning)
                m_Server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_ServerConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    Random rd = new Random();

                    for (int i = 0; i < 10; i++)
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

                    for (int i = 0; i < 10; i++)
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
        public void TestPerformance()
        {
            if (!m_Server.IsRunning)
                m_Server.Start();

            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_ServerConfig.Port);

            using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                Stream socketStream = new NetworkStream(socket);
                using (StreamReader reader = new StreamReader(socketStream, m_Encoding, true))
                using (StreamWriter writer = new StreamWriter(socketStream, m_Encoding, 1024 * 8))
                {
                    reader.ReadLine();

                    Random rd = new Random();

                    for (int i = 0; i < 10; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "ADD", x, y);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Assert.AreEqual(x + y, int.Parse(line));
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "MULT", x, y);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Assert.AreEqual(x * y, int.Parse(line));
                    }

                    var watch = new Stopwatch();

                    watch.Start();

                    for (int i = 0; i < 500; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "ADD", x, y);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Assert.AreEqual(x + y, int.Parse(line));
                    }

                    for (int i = 0; i < 500; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "MULT", x, y);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Assert.AreEqual(x * y, int.Parse(line));
                    }

                    watch.Stop();

                    var dynamicTime = watch.ElapsedMilliseconds;

                    watch.Reset();
                    watch.Start();

                    for (int i = 0; i < 500; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "ADDCS", x, y);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Assert.AreEqual(x + y, int.Parse(line));
                    }

                    for (int i = 0; i < 500; i++)
                    {
                        int x = rd.Next(1, 1000), y = rd.Next(1, 1000);
                        string command = string.Format("{0} {1} {2}", "MULTCS", x, y);
                        writer.WriteLine(command);
                        writer.Flush();
                        string line = reader.ReadLine();
                        Assert.AreEqual(x * y, int.Parse(line));
                    }

                    watch.Stop();
                    var staticTime = watch.ElapsedMilliseconds;

                    Console.WriteLine("{0}/{1} = {2}", dynamicTime, staticTime, ((decimal)dynamicTime / (decimal)staticTime).ToString("0.0"));
                }
            }
        }
    }
}
