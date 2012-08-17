using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SuperSocket.QuickStart.TerminatorProtocol
{
    [TestFixture]
    public class UnitTest
    {
        private IBootstrap m_Bootstrap;
        private int m_Port;

        [TestFixtureSetUp]
        public void Setup()
        {
            m_Bootstrap = BootstrapFactory.CreateBootstrapFromConfigFile("SuperSocket.config");
            Assert.IsTrue(m_Bootstrap.Initialize());
            m_Port = ((IAppServer) m_Bootstrap.AppServers.FirstOrDefault()).Config.Port;
            Assert.AreEqual(StartResult.Success, m_Bootstrap.Start());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            m_Bootstrap.Stop();
            m_Bootstrap = null;
        }

        private void RunTestCase(Action<StreamReader, StreamWriter> testcase)
        {
            var serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Port);

            using (var socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(serverAddress);
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, new ASCIIEncoding(), true))
                using (var writer = new StreamWriter(socketStream, new ASCIIEncoding(), 1024 * 8))
                {
                    testcase(reader, writer);
                }
            }
        }
        
        [Test]
        public void TestEchoNormal()
        {
            RunTestCase((reader, writer) =>
            {
                for (var i = 0; i < 25; i++)
                {
                    var requestLine = Guid.NewGuid().ToString();
                    writer.Write("ECHO ");
                    writer.Write(requestLine);
                    writer.Write("##");
                    writer.Flush();

                    Assert.AreEqual(requestLine, reader.ReadLine());
                }
            });
        }

        [Test]
        public void TestEchoBreak()
        {
            RunTestCase((reader, writer) =>
            {
                var lines = new List<string>();

                var stringBuilder = new StringBuilder();

                for (var i = 0; i < 10; i++)
                {
                    var requestLine = Guid.NewGuid().ToString();
                    lines.Add(requestLine);
                    stringBuilder.Append("ECHO ");
                    stringBuilder.Append(requestLine);
                    stringBuilder.Append("##");
                }

                var source = stringBuilder.ToString();

                var rd = new Random();

                for (var i = 0; i < 2; i++)
                {
                    var toBeSent = source.Length;
                    var totalSent = 0;

                    while (toBeSent > 0)
                    {
                        var thisSend = rd.Next(1, toBeSent);

                        writer.Write(source.Substring(totalSent, thisSend));
                        writer.Flush();
                        totalSent += thisSend;
                        toBeSent = source.Length - totalSent;

                        Thread.Sleep(10);
                    }

                    Thread.Sleep(500);

                    for (var j = 0; j < lines.Count; j++)
                    {
                        var line = reader.ReadLine();
                        Assert.AreEqual(lines[j], line, j.ToString());
                    }
                }
            });
        }

        [Test]
        public void TestEchoCombine()
        {
            RunTestCase((reader, writer) =>
            {
                var lines = new List<string>();

                var stringBuilder = new StringBuilder();

                for (var i = 0; i < 10; i++)
                {
                    var requestLine = Guid.NewGuid().ToString();
                    lines.Add(requestLine);
                    stringBuilder.Append("ECHO ");
                    stringBuilder.Append(requestLine);
                    stringBuilder.Append("##");
                }

                var source = stringBuilder.ToString();

                var rd = new Random();

                for (var i = 0; i < 2; i++)
                {
                    var toBeSent = source.Length;
                    var totalSent = 0;

                    while (toBeSent > 0)
                    {
                        var thisSend = rd.Next(1, toBeSent);

                        writer.Write(source.Substring(totalSent, thisSend));
                        totalSent += thisSend;
                        toBeSent = source.Length - totalSent;
                    }

                    writer.Flush();

                    for (var j = 0; j < lines.Count; j++)
                    {
                        var line = reader.ReadLine();
                        Assert.AreEqual(lines[j], line, j.ToString());
                    }
                }
            });
        }


        [Test]
        public void TestEchoLarge()
        {
            RunTestCase((reader, writer) =>
            {
                var lines = new List<string>();

                var stringBuilder = new StringBuilder();

                stringBuilder.Append("ECHO ");

                for (var i = 0; i < 10; i++)
                {
                    stringBuilder.Append(Guid.NewGuid().ToString());
                }

                stringBuilder.Append("##");

                var source = stringBuilder.ToString();

                var rd = new Random();

                for (var i = 0; i < 5; i++)
                {
                    writer.Write(source);
                    writer.Flush();

                    var line = reader.ReadLine();
                    Assert.AreEqual(source.Substring(5, source.Length - 7), line);
                }
            });
        }
    }
}
