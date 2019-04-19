using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public abstract class ProtocolTestBase : TestBase, IDisposable
    {
        readonly IServer _server;

        protected ProtocolTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _server = CreateServer();
            _server.StartAsync().Wait();
        }

        protected abstract IServer CreateServer();

        protected Socket CreateClient()
        {
            var serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040);
            var socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(serverAddress);

            return socket;
        }

        protected abstract string CreateRequest(string sourceLine);

        [Fact]
        public virtual void TestNormalRequest()
        {
            using (var socket = CreateClient())
            {
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, Utf8Encoding, true))
                using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                {
                    var line = Guid.NewGuid().ToString();
                    writer.Write(CreateRequest(line));
                    writer.Flush();

                    var receivedLine = reader.ReadLine();

                    Assert.Equal(line, receivedLine);
                }
            }
        }

        [Fact]
        public virtual void TestMiddleBreak()
        {
            for (var i = 0; i < 100; i++)
            {
                using (var socket = CreateClient())
                {
                    var socketStream = new NetworkStream(socket);
                    using (var reader = new StreamReader(socketStream, Utf8Encoding, true))
                    using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                    {
                        var line = Guid.NewGuid().ToString();
                        var sendingLine = CreateRequest(line);
                        writer.Write(sendingLine.Substring(0, sendingLine.Length / 2));
                        writer.Flush();
                    }
                }
            }
        }

        [Fact]
        public virtual void TestFragmentRequest()
        {
            using (var socket = CreateClient())
            {
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, Utf8Encoding, true))
                using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                {
                    var line = Guid.NewGuid().ToString();
                    var request = CreateRequest(line);

                    for (var i = 0; i < request.Length; i++)
                    {
                        writer.Write(request[i]);
                        writer.Flush();
                        Thread.Sleep(50);
                    }

                    var receivedLine = reader.ReadLine();
                    Assert.Equal(line, receivedLine);
                }
            }
        }

        [Fact]
        public virtual void TestBatchRequest()
        {
            using (var socket = CreateClient())
            {
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, Utf8Encoding, true))
                using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                {
                    int size = 100;

                    var lines = new string[size];

                    for (var i = 0; i < size; i++)
                    {
                        var line = Guid.NewGuid().ToString();
                        lines[i] = line;
                        var request = CreateRequest(line);
                        writer.Write(request);
                    }

                    writer.Flush();

                    for (var i = 0; i < size; i++)
                    {
                        var receivedLine = reader.ReadLine();
                        Assert.Equal(lines[i], receivedLine);
                    }
                }
            }
        }

        [Fact]
        public virtual void TestBreakRequest()
        {
            using (var socket = CreateClient())
            {
                var socketStream = new NetworkStream(socket);
                using (var reader = new StreamReader(socketStream, Utf8Encoding, true))
                using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                {
                    int size = 1000;

                    var lines = new string[size];

                    var sb = new StringBuilder();

                    for (var i = 0; i < size; i++)
                    {
                        var line = Guid.NewGuid().ToString();
                        lines[i] = line;
                        sb.Append(CreateRequest(line));
                    }

                    var source = sb.ToString();

                    var rd = new Random();

                    var rounds = new List<KeyValuePair<int, int>>();

                    var rest = source.Length;

                    while (rest > 0)
                    {
                        if (rest == 1)
                        {
                            rounds.Add(new KeyValuePair<int, int>(source.Length - rest, 1));
                            rest = 0;
                            break;
                        }

                        var thisRound = rd.Next(1, rest);
                        rounds.Add(new KeyValuePair<int, int>(source.Length - rest, thisRound));
                        rest -= thisRound;
                    }

                    for (var i = 0; i < rounds.Count; i++)
                    {
                        var r = rounds[i];
                        writer.Write(source.Substring(r.Key, r.Value));
                        writer.Flush();
                    }

                    for (var i = 0; i < size; i++)
                    {
                        var receivedLine = reader.ReadLine();
                        Assert.Equal(lines[i], receivedLine);
                    }
                }
            }
        }

        public void Dispose()
        {
            _server.StopAsync().Wait();
        }
    }
}
