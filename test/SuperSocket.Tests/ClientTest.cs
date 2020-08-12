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

namespace SuperSocket.Tests
{
    [Trait("Category", "Client")]
    public class ClientTest : TestClassBase
    {
        public ClientTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        class SecureEasyClient<TReceivePackage> : EasyClient<TReceivePackage>
            where TReceivePackage : class
        {
            public SecureEasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger)
                : this(pipelineFilter, new ChannelOptions { Logger = logger })
            {

            }

            public SecureEasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions options)
                : base(pipelineFilter, options)
            {

            }

            protected override IConnector GetConntector()
            {
                var authOptions = new SslClientAuthenticationOptions();
                authOptions.EnabledSslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12;
                authOptions.TargetHost = IPAddress.Loopback.ToString();
                authOptions.RemoteCertificateValidationCallback += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    return true;
                };

                return new SocketConnector(new SslStreamConnector(authOptions));
            }
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

                IEasyClient<TextPackageInfo> client;

                var services = new ServiceCollection();
                services.AddLogging();
                services.Configure<ILoggingBuilder>((loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                });

                var sp = services.BuildServiceProvider();

                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Client");

                var options = new ChannelOptions
                {
                    Logger = logger,
                    ReadAsDemand = clientReadAsDemand
                };
                
                if (hostConfigurator.IsSecure)
                    client = (new SecureEasyClient<TextPackageInfo>(new LinePipelineFilter(), options)).AsClient();
                else
                    client = new EasyClient<TextPackageInfo>(new LinePipelineFilter(), options).AsClient();               

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

                IEasyClient<StringPackageInfo> client;

                var services = new ServiceCollection();
                services.AddLogging();
                services.Configure<ILoggingBuilder>((loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                });

                var sp = services.BuildServiceProvider();

                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Client");

                var pipelineFilter = new CommandLinePipelineFilter
                {
                    Decoder = new DefaultStringPackageDecoder()
                };
                
                if (hostConfigurator.IsSecure)
                    client = (new SecureEasyClient<StringPackageInfo>(pipelineFilter, logger)).AsClient();
                else
                    client = new EasyClient<StringPackageInfo>(pipelineFilter, logger).AsClient();

                StringPackageInfo package = null;

                client.PackageHandler += (s, p) =>
                {
                    package = p;
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

                var services = new ServiceCollection();
                services.AddLogging();
                services.Configure<ILoggingBuilder>((loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                });

                var sp = services.BuildServiceProvider();

                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Client");

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4040));
                var stream = await hostConfigurator.GetClientStream(socket);

                var channel = new StreamPipeChannel<TextPackageInfo>(stream, socket.RemoteEndPoint, socket.LocalEndPoint, new LinePipelineFilter(), new ChannelOptions
                {
                    Logger = logger,
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
