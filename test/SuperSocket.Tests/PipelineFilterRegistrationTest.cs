using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Host;
using Xunit;
using System.Threading;
using System.Buffers;
using Microsoft.Extensions.Hosting;
using Meziantou.Extensions.Logging.Xunit.v3;
using SuperSocket.Server.Abstractions;
using System.Collections.Generic;

namespace SuperSocket.Tests
{
    [Trait("Category", "PipelineFilterDI")]
    public class PipelineFilterRegistrationTest : TestClassBase
    {
        public PipelineFilterRegistrationTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        // A service to be injected
        public class TestService
        {
            public string GetValue() => "TestValue";
        }

        // A filter without default constructor that requires DI
        public class FilterWithDependency : IPipelineFilter<TextPackageInfo>
        {
            private readonly TestService _testService;

            // Constructor that requires dependency injection
            public FilterWithDependency(TestService testService)
            {
                _testService = testService;
            }

            public IPackageDecoder<TextPackageInfo> Decoder { get; set; }

            public IPipelineFilter<TextPackageInfo> NextFilter => null;

            public object Context { get; set; }

            public TextPackageInfo Filter(ref SequenceReader<byte> reader)
            {
                if (!reader.TryReadTo(out ReadOnlySequence<byte> sequence, new byte[] { (byte)'\r', (byte)'\n' }, advancePastDelimiter: true))
                    return null;

                var text = sequence.GetString(Utf8Encoding);

                // Add a prefix from the injected service to prove DI is working
                return new TextPackageInfo { Text = $"{_testService.GetValue()}:{text}" };
            }

            public void Reset()
            {
            }
        }

        // A filter with default constructor
        public class FilterWithDefaultConstructor : IPipelineFilter<TextPackageInfo>
        {
            public FilterWithDefaultConstructor()
            {
            }

            public IPackageDecoder<TextPackageInfo> Decoder { get; set; }

            public IPipelineFilter<TextPackageInfo> NextFilter => null;

            public object Context { get; set; }

            public TextPackageInfo Filter(ref SequenceReader<byte> reader)
            {
                if (!reader.TryReadTo(out ReadOnlySequence<byte> sequence, new byte[] { (byte)'\r', (byte)'\n' }, advancePastDelimiter: true))
                    return null;

                var text = sequence.GetString(Utf8Encoding);

                // Add a prefix to indicate this is the default constructor filter
                return new TextPackageInfo { Text = $"DefaultConstructor:{text}" };
            }

            public void Reset()
            {
            }
        }

        [Fact]
        public async Task TestFilterWithDependencyInjection()
        {
            var serverSessionEvent = new AutoResetEvent(false);

            using (var server = CreateSocketServerBuilder<TextPackageInfo>()
                .ConfigureServices((hostCtx, services) =>
                {
                    // Register the service that will be injected into the filter
                    services.AddSingleton<TestService>();
                })
                .UsePipelineFilter<FilterWithDependency>()
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                })
                .UseSessionHandler(
                    onConnected: (s) =>
                    {
                        serverSessionEvent.Set();
                        return ValueTask.CompletedTask;
                    },
                    onClosed: (s, e) =>
                    {
                        serverSessionEvent.Set();
                        return ValueTask.CompletedTask;
                    })
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, DefaultServerPort));
                    Assert.True(serverSessionEvent.WaitOne(1000));

                    // Send test message
                    var message = "Hello World";
                    await client.SendAsync(Utf8Encoding.GetBytes(message + "\r\n"), SocketFlags.None);

                    // Receive response
                    var buffer = new byte[1024];
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Utf8Encoding.GetString(buffer, 0, received);

                    // Verify response contains service value to prove DI is working
                    Assert.Contains("TestValue", response);
                    Assert.Equal($"TestValue:{message}\r\n", response);

                    // Close the client
                    client.Close();
                    Assert.True(serverSessionEvent.WaitOne(1000));
                }

                await server.StopAsync();
            }
        }

        [Fact]
        public async Task TestFilterWithDefaultConstructor()
        {
            var serverSessionEvent = new AutoResetEvent(false);

            using (var server = CreateSocketServerBuilder<TextPackageInfo>()
                .UsePipelineFilter<FilterWithDefaultConstructor>()
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                })
                .UseSessionHandler(
                    onConnected: (s) =>
                    {
                        serverSessionEvent.Set();
                        return ValueTask.CompletedTask;
                    },
                    onClosed: (s, e) =>
                    {
                        serverSessionEvent.Set();
                        return ValueTask.CompletedTask;
                    })
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, DefaultServerPort));
                    Assert.True(serverSessionEvent.WaitOne(1000));

                    // Send test message
                    var message = "Hello World";
                    await client.SendAsync(Utf8Encoding.GetBytes(message + "\r\n"), SocketFlags.None);

                    // Receive response
                    var buffer = new byte[1024];
                    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Utf8Encoding.GetString(buffer, 0, received);

                    // Verify response is from the default constructor filter
                    Assert.Contains("DefaultConstructor", response);
                    Assert.Equal($"DefaultConstructor:{message}\r\n", response);

                    // Close the client
                    client.Close();
                    Assert.True(serverSessionEvent.WaitOne(1000));
                }

                await server.StopAsync();
            }
        }
    }
}