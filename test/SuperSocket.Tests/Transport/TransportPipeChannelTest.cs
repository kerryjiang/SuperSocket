using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;
using SuperSocket.Channel.Kestrel;
using SuperSocket.ProtoBase;
using Xunit;
using Xunit.Abstractions;

namespace SuperSocket.Tests
{
    [Trait("Category", "TransportPipeChannel")]
    public class TransportPipeChannelTest : TestClassBase
    {
        public TransportPipeChannelTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "TestKestrelConnectionContextChannel")]
        public async Task TestKestrelConnectionContextChannel(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                       .ConfigureServices((ctx, ser) => { ser.AddSocketConnectionFactory(); })
                       .UsePackageHandler(async (s, p) =>
                       {
                           await s.SendAsync(Utf8Encoding.GetBytes("PRE-" + p.Text + "\r\n"));
                       }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var connectionFactory = server.ServiceProvider.GetRequiredService<IConnectionFactory>();

                var connectionContext = await connectionFactory.ConnectAsync(hostConfigurator.GetServerEndPoint());

                var channel = new TransportPipeChannel<TextPackageInfo>(connectionContext.Transport,
                    connectionContext.LocalEndPoint, connectionContext.RemoteEndPoint, new LinePipelineFilter(),
                    new ChannelOptions
                    {
                        Logger = DefaultLoggerFactory.CreateLogger("TransportPipeChannel")
                    });

                channel.Start();

                var msg = Guid.NewGuid().ToString();
                await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

                var round = 0;

                await foreach (var package in channel.RunAsync())
                {
                    Assert.NotNull(package);
                    Assert.Equal("PRE-" + msg, package.Text);
                    round++;

                    if (round >= 10)
                        break;

                    msg = Guid.NewGuid().ToString();
                    await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "TestKestrelConnectionContextChannelClose")]
        public async Task TestKestrelConnectionContextChannelClose(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                       .ConfigureServices((ctx, ser) => { ser.AddSocketConnectionFactory(); })
                       .UsePackageHandler(async (s, p) =>
                       {
                           await s.SendAsync(Utf8Encoding.GetBytes("PRE-" + p.Text + "\r\n"));
                       }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var connectionFactory = server.ServiceProvider.GetRequiredService<IConnectionFactory>();

                var connectionContext = await connectionFactory.ConnectAsync(hostConfigurator.GetServerEndPoint());

                var channel = new TransportPipeChannel<TextPackageInfo>(connectionContext.Transport,
                    connectionContext.LocalEndPoint, connectionContext.RemoteEndPoint, new LinePipelineFilter(),
                    new ChannelOptions
                    {
                        Logger = DefaultLoggerFactory.CreateLogger("TransportPipeChannel")
                    });

                channel.Start();
                
                var msg = Guid.NewGuid().ToString();
                await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

                var round = 0;

                await foreach (var package in channel.RunAsync())
                {
                    Assert.NotNull(package);
                    Assert.Equal("PRE-" + msg, package.Text);
                    round++;

                    if (round >= 20)
                        break;

                    msg = Guid.NewGuid().ToString();
                    await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));
                }
                
                await channel.CloseAsync(CloseReason.LocalClosing);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "TestKestrelConnectionContextChannelClose")]
        public async Task TestKestrelConnectionContextChannelCloseOnetime(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                       .ConfigureServices((ctx, ser) => { ser.AddSocketConnectionFactory(); })
                       .UsePackageHandler(async (s, p) =>
                       {
                           await s.SendAsync(Utf8Encoding.GetBytes("PRE-" + p.Text + "\r\n"));
                       }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var closedTime = 0;
                var connectionFactory = server.ServiceProvider.GetRequiredService<IConnectionFactory>();

                var connectionContext = await connectionFactory.ConnectAsync(hostConfigurator.GetServerEndPoint());

                var channel = new TransportPipeChannel<TextPackageInfo>(connectionContext.Transport,
                    connectionContext.LocalEndPoint, connectionContext.RemoteEndPoint, new LinePipelineFilter(),
                    new ChannelOptions
                    {
                        Logger = DefaultLoggerFactory.CreateLogger("TransportPipeChannel")
                    });

                channel.Closed += (sender, args) =>
                {
                    closedTime++;
                };
                
                channel.Start();
                
                var msg = Guid.NewGuid().ToString();
                await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

                var round = 0;

                await foreach (var package in channel.RunAsync())
                {
                    Assert.NotNull(package);
                    Assert.Equal("PRE-" + msg, package.Text);
                    round++;

                    if (round >= 20)
                        break;

                    msg = Guid.NewGuid().ToString();
                    await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));
                }
                
                await channel.CloseAsync(CloseReason.LocalClosing);

                await Task.Delay(1000);
                
                Assert.True(closedTime == 1);
                
                await server.StopAsync();
            }
        }
        
        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "TestDetachableKestrelConnectionContextChannel")]
        public async Task TestDetachableKestrelConnectionContextChannel(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                       .ConfigureServices((ctx, ser) => { ser.AddSocketConnectionFactory(); })
                       .UsePackageHandler(async (s, p) =>
                       {
                           await s.SendAsync(Utf8Encoding.GetBytes("PRE-" + p.Text + "\r\n"));
                       }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var connectionFactory = server.ServiceProvider.GetRequiredService<IConnectionFactory>();

                var connectionContext = await connectionFactory.ConnectAsync(hostConfigurator.GetServerEndPoint());

                var channel = new TransportPipeChannel<TextPackageInfo>(connectionContext.Transport,
                    connectionContext.LocalEndPoint, connectionContext.RemoteEndPoint, new LinePipelineFilter(),
                    new ChannelOptions
                    {
                        Logger = DefaultLoggerFactory.CreateLogger("TransportPipeChannel"),
                        ReadAsDemand = true,
                    });

                channel.Start();

                var msg = Guid.NewGuid().ToString();
                await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

                var round = 0;

                await foreach (var package in channel.RunAsync())
                {
                    Assert.NotNull(package);
                    Assert.Equal("PRE-" + msg, package.Text);
                    round++;

                    if (round >= 1)
                        break;

                    msg = Guid.NewGuid().ToString();
                    await channel.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));
                }

                OutputHelper.WriteLine("Before DetachAsync");

                await channel.DetachAsync();

                // the connection is still alive in the server
                Assert.Equal(1, server.SessionCount);

                // socket.Connected is is still connected
                Assert.False(connectionContext.ConnectionClosed.IsCancellationRequested);

                var reader = connectionContext.Transport.Input;
                var writer = connectionContext.Transport.Output;

                for (var i = 0; i < 10; i++)
                {
                    var line = Guid.NewGuid().ToString() + "\r\n";
                    writer.Write(Utf8Encoding.GetBytes(line));
                    await writer.FlushAsync();

                    OutputHelper.WriteLine($"Sent {(i + 1)} message over the detached network stream");

                    ReadResult readResult = default;

                    try
                    {
                        readResult = await reader.ReadAsync();

                        var buffer = readResult.Buffer;
                        var result = buffer.GetString(Utf8Encoding);

                        Assert.Equal("PRE-" + line, result);
                        OutputHelper.WriteLine($"Received {(i + 1)} message over the detached network stream");
                    }
                    finally
                    {
                        reader.AdvanceTo(readResult.Buffer.End);
                    }
                }

                await server.StopAsync();
            }
        }
    }
}