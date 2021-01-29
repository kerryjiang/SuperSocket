using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket;
using SuperSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace SuperSocket.Tests
{
    public abstract class ProtocolTestBase : TestClassBase
    {
        
        protected ProtocolTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
        {
  
        }

        protected abstract IServer CreateServer(IHostConfigurator hostConfigurator);
        
        protected abstract string CreateRequest(string sourceLine);

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public virtual async Task TestNormalRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                    using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                    {
                        var line = Guid.NewGuid().ToString();
                        writer.Write(CreateRequest(line));
                        writer.Flush();

                        var receivedLine = reader.ReadLine();
                        Assert.Equal(line, receivedLine);
                    }
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public virtual async Task TestMiddleBreak(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                for (var i = 0; i < 100; i++)
                {
                    using (var socket = CreateClient(hostConfigurator))
                    {
                        using (var socketStream = await hostConfigurator.GetClientStream(socket))
                        using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                        using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                        {
                            var line = Guid.NewGuid().ToString();
                            var sendingLine = CreateRequest(line);
                            writer.Write(sendingLine.Substring(0, sendingLine.Length / 2));
                            writer.Flush();
                            await hostConfigurator.KeepSequence();
                        }
                    }
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public virtual async Task TestFragmentRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                    using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                    {
                        var line = Guid.NewGuid().ToString();
                        var request = CreateRequest(line);

                        for (var i = 0; i < request.Length; i++)
                        {
                            writer.Write(request[i]);
                            writer.Flush();
                            Thread.Sleep(50);
                            await hostConfigurator.KeepSequence();
                        }

                        var receivedLine = reader.ReadLine();
                        Assert.Equal(line, receivedLine);
                    }
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public virtual async Task TestBatchRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
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
                            await hostConfigurator.KeepSequence();
                        }

                        writer.Flush();
                        await hostConfigurator.KeepSequence();

                        for (var i = 0; i < size; i++)
                        {
                            var receivedLine = reader.ReadLine();
                            Assert.Equal(lines[i], receivedLine);
                        }
                    }
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public virtual async Task TestBreakRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
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
                            await hostConfigurator.KeepSequence();
                        }

                        for (var i = 0; i < size; i++)
                        {
                            var receivedLine = reader.ReadLine();
                            Assert.Equal(lines[i], receivedLine);
                        }
                    }
                }

                await server.StopAsync();
            }
        }
    }
}
