using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using SuperSocket.Dlr;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;

namespace SuperSocket.Test
{
    [TestFixture]
    public class DLRSocketServerTest : BootstrapTestBase
    {
        private Encoding m_Encoding = new UTF8Encoding();

        [Test]
        public void TestCommands()
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
