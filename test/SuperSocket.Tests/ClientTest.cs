using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Xunit.Abstractions;
using SuperSocket.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.Channel;
using SuperSocket.Tests.Command;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;

namespace SuperSocket.Tests
{
    [Trait("Category", "Client")]
    public class ClientTest : TestClassBase
    {
        public ClientTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Theory]
        [Trait("Category", "Client.TestEcho")]
        [InlineData(typeof(RegularHostConfigurator), false)]
        [InlineData(typeof(SecureHostConfigurator), false)]
        [InlineData(typeof(RegularHostConfigurator), true)]
        public async Task TestEcho(Type hostConfiguratorType, bool clientReadAsDemand)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var options = new ChannelOptions
                {
                    Logger = NullLogger.Instance,
                    ReadAsDemand = clientReadAsDemand
                };
                
                var client = hostConfigurator.ConfigureEasyClient(new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options));

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                for (var i = 0; i < 100; i++)
                {
                    var msg = Guid.NewGuid().ToString();
                    await client.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

                    var package = await client.ReceiveAsync();
                    Assert.NotNull(package);
                    Assert.Equal(msg, package.Text); 
                }

                await client.CloseAsync();
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "Client.TestBindLocalEndPoint")]
        public async Task TestBindLocalEndPoint(Type hostConfiguratorType)
        {
            IAppSession session = default;

            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
            .UseSessionHandler(async s =>
            {
                session = s;
                await Task.CompletedTask;
            })
            .BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");                

                var pipelineFilter = new CommandLinePipelineFilter
                {
                    Decoder = new DefaultStringPackageDecoder()
                };

                var options = new ChannelOptions
                {
                    Logger = DefaultLoggerFactory.CreateLogger(nameof(TestBindLocalEndPoint))
                };
                
                var client = hostConfigurator.ConfigureEasyClient(new EasyClient<StringPackageInfo>(pipelineFilter, options));
                
                var localPort = 8080;

                client.LocalEndPoint = new IPEndPoint(IPAddress.Loopback, localPort);

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                await Task.Delay(500);

                Assert.NotNull(session);
                Assert.Equal(localPort, (session.RemoteEndPoint as IPEndPoint).Port);

                await client.CloseAsync();
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommandLine(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
            .UseCommand((options) =>
            {
                options.AddCommand<SORT>();
            }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var pipelineFilter = new CommandLinePipelineFilter
                {
                    Decoder = new DefaultStringPackageDecoder()
                };
                
                var client = hostConfigurator.ConfigureEasyClient(new EasyClient<StringPackageInfo>(pipelineFilter));

                StringPackageInfo package = null;

                client.PackageHandler += async (s, p) =>
                {
                    package = p;
                    await Task.CompletedTask;
                };

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                client.StartReceive();

                await client.SendAsync(Utf8Encoding.GetBytes("SORT 10 7 3 8 6 43 23\r\n"));
                await Task.Delay(1000);

                Assert.NotNull(package);

                Assert.Equal("SORT", package.Key);
                Assert.Equal("3 6 7 8 10 23 43", package.Body);

                await client.CloseAsync();
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "TestDetachableChannel")]
        public async Task TestDetachableChannel(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes("PRE-" + p.Text + "\r\n"));
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(hostConfigurator.GetServerEndPoint());
                var stream = await hostConfigurator.GetClientStream(socket);

                var channel = new StreamPipeChannel<TextPackageInfo>(stream, socket.RemoteEndPoint, socket.LocalEndPoint, new LinePipelineFilter(), new ChannelOptions
                {
                    Logger = NullLogger.Instance,
                    ReadAsDemand = true
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


                OutputHelper.WriteLine("Before DetachAsync");

                await channel.DetachAsync();
                
                // the connection is still alive in the server
                Assert.Equal(1, server.SessionCount);

                // socket.Connected is is still connected
                Assert.True(socket.Connected);

                var ns = stream as DerivedNetworkStream;
                Assert.True(ns.Socket.Connected);

                // the stream is still usable
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var txt = Guid.NewGuid().ToString();
                        await streamWriter.WriteAsync(txt + "\r\n");
                        await streamWriter.FlushAsync();
                        var line = await streamReader.ReadLineAsync();
                        Assert.Equal("PRE-" + txt, line);
                    }
                }

                await server.StopAsync();
            }
        }
    }
}
