using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using Xunit;
using Xunit.Abstractions;
using SuperSocket.Client;
using SuperSocket.Server.Host;
using SuperSocket.Tests.Command;
using SuperSocket.Connection;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.WebSocket;

namespace SuperSocket.Tests
{
    [Trait("Category", "Client")]
    public class ClientTest : TestClassBase
    {
        private static Random _rd = new Random();
        
        public ClientTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }
        
        [Theory]
        [Trait("Category", "Client.TestEcho")]
        [InlineData(typeof(RegularHostConfigurator), false)]
        [InlineData(typeof(SecureHostConfigurator), false)]
        [InlineData(typeof(GzipHostConfigurator), false)]
        [InlineData(typeof(GzipSecureHostConfigurator), false)]
        [InlineData(typeof(RegularHostConfigurator), true)]
        public async Task TestEcho(Type hostConfiguratorType, bool clientReadAsDemand)
        {
            var serverSessionEvent = new AutoResetEvent(false);

            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
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

                var options = new ConnectionOptions
                {
                    Logger = NullLogger.Instance,
                    ReadAsDemand = clientReadAsDemand
                };
                
                var client = hostConfigurator.ConfigureEasyClient(new LinePipelineFilter(), options);

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                Assert.True(serverSessionEvent.WaitOne(1000));

                for (var i = 0; i < 100; i++)
                {
                    var msg = Guid.NewGuid().ToString();
                    await client.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

                    var package = await client.ReceiveAsync();
                    Assert.NotNull(package);
                    Assert.Equal(msg, package.Text); 
                }

                await client.CloseAsync();
                Assert.True(serverSessionEvent.WaitOne(1000));
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(GzipHostConfigurator))]
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

                var options = new ConnectionOptions
                {
                    Logger = DefaultLoggerFactory.CreateLogger(nameof(TestBindLocalEndPoint))
                };
                
                var client = hostConfigurator.ConfigureEasyClient(pipelineFilter, options);
                var connected = false;
                var localPort = 0;
                
                for (var i = 0; i < 3; i++)
                {
                    localPort = _rd.Next(40000, 50000);
                    client.LocalEndPoint = new IPEndPoint(IPAddress.Loopback, localPort);

                    try
                    {
                        connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode == SocketError.AccessDenied || e.SocketErrorCode == SocketError.AddressAlreadyInUse)
                            continue;
                        
                        throw e;
                    }

                    break;                    
                }                
                
                Assert.True(connected);

                await Task.Delay(500);

                Assert.NotNull(session);
                Assert.Equal(localPort, (session.RemoteEndPoint as IPEndPoint).Port);

                await client.CloseAsync();
                await server.StopAsync();
            }
        }

        [Fact]
        public void TestCancellationTokenIsBeingUsedWhenConnecting()
        {
            var pipelineFilter = new CommandLinePipelineFilter
            {
                Decoder = new DefaultStringPackageDecoder()
            };

            var options = new ConnectionOptions
            {
                Logger = DefaultLoggerFactory.CreateLogger(nameof(TestBindLocalEndPoint))
            };
            var ea = new EasyClient<WebSocketPackage>(new WebSocketPipelineFilter());
            var client = ea.AsClient();
            var localPort = 0;

            localPort = _rd.Next(40000, 50000);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1));
            var result = client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, localPort), cts.Token).AsTask();
            int index = Task.WaitAny(new[] { result }, TimeSpan.FromSeconds(2));
            var hasWaitedLongerThanTheCancellationToken = index == -1;

            Assert.False(hasWaitedLongerThanTheCancellationToken);
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(GzipSecureHostConfigurator))]
        [InlineData(typeof(GzipHostConfigurator))]
        public async Task TestCommandLine(Type hostConfiguratorType)
        {
            var packageEvent = new AutoResetEvent(false);

            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
            .UseCommand((options) =>
            {
                options.AddCommand<SORT>();
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

                var options = new ConnectionOptions
                {
                    Logger = DefaultLoggerFactory.CreateLogger(nameof(TestCommandLine))
                };
                var client = hostConfigurator.ConfigureEasyClient(pipelineFilter, options);

                StringPackageInfo package = null;

                client.PackageHandler += async (s, p) =>
                {
                    package = p;
                    packageEvent.Set();
                    await Task.CompletedTask;
                };

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                client.StartReceive();

                for (var i = 0; i < 5; i++)
                {
                    await client.SendAsync(Utf8Encoding.GetBytes("SORT 10 7 3 8 6 43 23\r\n"));

                    Assert.True(packageEvent.WaitOne(1000));
                    Assert.NotNull(package);

                    Assert.Equal("SORT", package.Key);
                    Assert.Equal("3 6 7 8 10 23 43", package.Body);
                }

                await client.CloseAsync();
                await server.StopAsync();
            }
        }

        [Fact]
        [Trait("Category", "TestDetachableChannel")]
        public async Task TestDetachableChannel()
        {
            IHostConfigurator hostConfigurator = new RegularHostConfigurator();

            await TestDetachableChannelInternal(hostConfigurator, (_, socket) =>
                new StreamPipeConnection(
                    hostConfigurator.GetClientStream(socket).Result,
                    socket.RemoteEndPoint,
                    socket.LocalEndPoint,
                    new ConnectionOptions
                    {
                        Logger = DefaultLoggerFactory.CreateLogger(nameof(TestDetachableChannel)),
                        ReadAsDemand = true
                    })
                );

            /* KestrelPipeConnection doesn't support Detach right now.
            await TestDetachableChannelInternal(new KestralConnectionHostConfigurator(), (server, socket) =>
                new KestrelPipeConnection(
                        server.ServiceProvider.GetService<SocketConnectionContextFactory>().Create(socket),
                        new ConnectionOptions
                        {
                            Logger = DefaultLoggerFactory.CreateLogger(nameof(TestDetachableChannel)),
                            ReadAsDemand = false
                        }
                    )
                );
                */
        }

        async Task TestDetachableChannelInternal(IHostConfigurator hostConfigurator, Func<IServer, Socket, IConnection> connectionFactory)
        {
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes("PRE-" + p.Text + "\r\n"));
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                using (var socket = hostConfigurator.CreateClient())
                {
                    var connection = connectionFactory(server, socket);

                    await TestConnection(connection);

                    OutputHelper.WriteLine("Before DetachAsync");

                    await connection.DetachAsync();

                    // the connection is still alive in the server
                    Assert.Equal(1, server.SessionCount);

                    // socket.Connected is is still connected
                    Assert.True(socket.Connected);

                    // Attach the socket with another connection
                    connection = connectionFactory(server, socket);

                    await TestConnection(connection);
                }                

                await server.StopAsync();
            }
        }

        async Task TestConnection(IConnection connection)
        {
            var packagePipe = connection.RunAsync(new LinePipelineFilter());

            var msg = Guid.NewGuid().ToString();
            await connection.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));

            var round = 0;

            await foreach (var package in packagePipe)
            {
                Assert.NotNull(package);
                Assert.Equal("PRE-" + msg, package.Text);
                round++;

                if (round >= 10)
                    break;

                msg = Guid.NewGuid().ToString();
                await connection.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));
            }
        }
    }
}
