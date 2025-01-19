using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Host;
using Xunit;

namespace SuperSocket.Tests
{
    [Trait("Category", "Protocol.BeginEndMark")]
    public class BeginEndMarkProtocolTest : ProtocolTestBase
    {
        public BeginEndMarkProtocolTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        class MyMarkHeaderPipelineFilter : BeginEndMarkPipelineFilter<TextPackageInfo>
        {
            public MyMarkHeaderPipelineFilter()
                : base(new byte[] { 0x0B }, new byte[] { 0x1C, 0x0D })
            {

            }

            protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
            {
                return new TextPackageInfo { Text = buffer.GetString(Utf8Encoding) };
            }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return sourceLine.Length.ToString().PadLeft(4) + sourceLine;
        }

        protected override IServer CreateServer(IHostConfigurator hostConfigurator)
        {
            return CreateSocketServerBuilder<TextPackageInfo, MyMarkHeaderPipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer() as IServer;
        }

        private void WritePackage(Stream stream, string line)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(3 + Utf8Encoding.GetMaxByteCount(line.Length));
            var pack = EncodePackage(line, buffer);
            stream.Write(pack);
            pool.Return(buffer);
        }

        private void WriteHalfPackage(Stream stream, string line)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(3 + Utf8Encoding.GetMaxByteCount(line.Length));
            var pack = EncodePackage(line, buffer);
            stream.Write(pack.Slice(0, pack.Length / 2));
            pool.Return(buffer);
        }

        private void WriteFragmentPackage(Stream stream, string line)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(3 + Utf8Encoding.GetMaxByteCount(line.Length));
            var pack = EncodePackage(line, buffer);

            for (var i = 0; i < pack.Length; i++)
            {
                stream.Write(buffer, i, 1);
                stream.Flush();
                Thread.Sleep(50);
            }

            pool.Return(buffer);
        }

        private void WriteMultiplePackages(IHostConfigurator hostConfigurator, Stream stream, string[] lines)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(lines.Sum(x => 3 + Utf8Encoding.GetMaxByteCount(x.Length)));

            Span<byte> span = buffer;

            var total = 0;

            foreach (var line in lines)
            {
                var pack = EncodePackage(line, span);
                span = span.Slice(pack.Length);
                total += pack.Length;
            }

            span = new Span<byte>(buffer, 0, total);

            var rd = new Random();
            var maxRd = total / 2;

            while (span.Length > 0)
            {
                var size = rd.Next(1, maxRd);
                size = Math.Min(size, span.Length);

                stream.Write(span.Slice(0, size));
                stream.Flush();
                hostConfigurator.KeepSequence().GetAwaiter().GetResult();

                span = span.Slice(size);
            }

            pool.Return(buffer);
        }

        private ReadOnlySpan<byte> EncodePackage(string line, Span<byte> span)
        {
            span[0] = 0x0B;

            var len = Utf8Encoding.GetBytes(line.AsSpan(), span.Slice(1));
            var rest = span.Slice(1 + len);

            rest[0] = 0x1C;
            rest[1] = 0x0D;

            return span.Slice(0, len + 3);
        }


        public override async Task TestNormalRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                    {
                        var line = Guid.NewGuid().ToString();

                        WritePackage(socketStream, line);

                        await socketStream.FlushAsync();

                        var receivedLine = await reader.ReadLineAsync();

                        Assert.Equal(line, receivedLine);
                    }
                }

                await server.StopAsync();
            }
        }

        public override async Task TestMiddleBreak(Type hostConfiguratorType)
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
                        {
                            var line = Guid.NewGuid().ToString();
                            WriteHalfPackage(socketStream, line);
                            await socketStream.FlushAsync();
                        }
                    }
                }

                await server.StopAsync();
            }
        }

        public override async Task TestFragmentRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                    {
                        var line = Guid.NewGuid().ToString();

                        WriteFragmentPackage(socketStream, line);

                        var receivedLine = await reader.ReadLineAsync();
                        Assert.Equal(line, receivedLine);
                    }
                }

                await server.StopAsync();
            }
        }

        public override async Task TestBatchRequest(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateServer(hostConfigurator))
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                    {
                        int size = 100;

                        var lines = new string[size];

                        for (var i = 0; i < size; i++)
                        {
                            var line = Guid.NewGuid().ToString();

                            lines[i] = line;
                            WritePackage(socketStream, line);
                            await hostConfigurator.KeepSequence();
                        }

                        socketStream.Flush();
                        await hostConfigurator.KeepSequence();

                        for (var i = 0; i < size; i++)
                        {
                            var receivedLine = await reader.ReadLineAsync();
                            Assert.Equal(lines[i], receivedLine);
                        }
                    }
                }

                await server.StopAsync();
            }
        }

        public override Task TestBreakRequest(Type hostConfiguratorType)
        {
            return Task.CompletedTask;
        }

        [Theory(Timeout = 60000)]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(GzipSecureHostConfigurator))]
        [InlineData(typeof(GzipHostConfigurator))]
        public async Task TestBreakRequest2(Type hostConfiguratorType)
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

                        for (var i = 0; i < size; i++)
                        {
                            var line = Guid.NewGuid().ToString();
                            lines[i] = line;
                        }

                        WriteMultiplePackages(hostConfigurator, socketStream, lines);

                        for (var i = 0; i < size; i++)
                        {
                            var receivedLine = await reader.ReadLineAsync();
                            Assert.Equal(lines[i], receivedLine);
                        }
                    }
                }

                await server.StopAsync();
            }
        }
    }
}
