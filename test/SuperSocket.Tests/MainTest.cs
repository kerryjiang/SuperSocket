using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting.Internal;
using System.Linq;
using System.Collections.Generic;
using System.Security.Authentication;

/// <summary>
/// Run selected test case by command
/// dotnet test --filter 'FullyQualifiedName=SuperSocket.Tests.SessionTest.TestCloseReason'
/// </summary>

namespace SuperSocket.Tests
{
    [Trait("Category", "Basic")]
    public class MainTest : TestClassBase
    {
        public MainTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
        }

        [Fact]
        public async Task TestSessionCount() 
        {
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes("Hello World\r\n"));
                }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                Assert.Equal(0, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetDefaultServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                Assert.Equal(1, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                await Task.Delay(1000);

                Assert.Equal(0, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                await server.StopAsync();
            }
        }

        [Fact]
        public void TestCustomConfigOptions() 
        {
            var hostConfigurator = new RegularHostConfigurator();
            var propName = "testPropName";
            var propValue = "testPropValue";

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .ConfigureAppConfiguration((HostBuilder, configBuilder) =>
                    {
                        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { $"serverOptions:values:{propName}", propValue }
                        });
                    }).BuildAsServer())
            {
                Assert.Equal(propValue, server.Options.Values[propName]);
            }
        }

        [Theory]
        [InlineData("Tls12", SslProtocols.Tls12, false)]
        [InlineData("Tls15", SslProtocols.None, true)]
        [InlineData("Tls11, Tls12", SslProtocols.Tls11 | SslProtocols.Tls12, false)]
        [InlineData("Tls11,Tls12", SslProtocols.Tls11 | SslProtocols.Tls12, false)]
        [InlineData("Tls11|Tls12", SslProtocols.Tls11 | SslProtocols.Tls12, true)]        
        public async Task TestSecurityOptions(string security, SslProtocols protocols, bool expectException) 
        {
            var hostConfigurator = new SecureHostConfigurator();
            var listener = default(ListenOptions);

            var createServer = new Func<IServer>(() =>
            {
                return CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                    .ConfigureAppConfiguration((HostBuilder, configBuilder) =>
                    {
                        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "serverOptions:listeners:0:security", security }
                        });
                    })
                    .ConfigureSuperSocket(serverOptions =>
                    {
                        listener = serverOptions.Listeners.FirstOrDefault();
                    })
                    .BuildAsServer();
            });

            IServer server = null;

            if (!expectException)
                server = createServer();
            else
            {
                var exce = Assert.ThrowsAny<Exception>(() => 
                {
                    server = createServer();
                });

                return;
            }
            
            Assert.NotNull(listener);
            Assert.Equal(protocols, listener.Security);

            using (server)
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                Assert.Equal(0, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                using (var socket = CreateClient(hostConfigurator))
                {
                    var socketStream = await hostConfigurator.GetClientStream(socket);
                    await Task.Delay(500);
                    Assert.Equal(1, server.SessionCount);
                    OutputHelper.WriteLine("SessionCount:" + server.SessionCount);              
                }

                await Task.Delay(500);
                Assert.Equal(0, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                await server.StopAsync();
            }            
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public async Task TestSessionHandlers(Type hostConfiguratorType) 
        {
            var connected = false;
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UseSessionHandler((s) =>
                {
                    connected = true;
                    return new ValueTask();
                }, (s, e) =>
                {
                    connected = false;
                    return new ValueTask();
                })
                .UsePackageHandler(async (s, p) =>
                {
                    if (p.Text == "CLOSE")
                        await s.CloseAsync(Channel.CloseReason.LocalClosing);            
                }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                var client = hostConfigurator.CreateClient();
                var outputStream = default(Stream);

                if (hostConfigurator is UdpHostConfigurator)
                {                    
                    var buffer = Encoding.ASCII.GetBytes("HELLO\r\n");
                    outputStream = await hostConfigurator.GetClientStream(client);
                    outputStream.Write(buffer, 0, buffer.Length);
                    outputStream.Flush();
                }

                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                Assert.True(connected);

                if (outputStream != null)
                {                    
                    var buffer = Encoding.ASCII.GetBytes("CLOSE\r\n");
                    outputStream.Write(buffer, 0, buffer.Length);
                    outputStream.Flush();
                }
                else
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }                

                await Task.Delay(1000);

                if (outputStream != null)
                {
                    client.Close();
                }

                Assert.False(connected);

                await server.StopAsync();
            }
        }

        [Fact]
        public async Task TestUseHostedService() 
        {
            var connected = false;

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .UseSessionHandler((s) =>
                {
                    connected = true;
                    return new ValueTask();
                }, (s, e) =>
                {
                    connected = false;
                    return new ValueTask();
                })
                .UseHostedService<SuperSocketServiceA>()
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetDefaultServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                Assert.True(connected);

                Assert.IsType<SuperSocketServiceA>(server);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                await Task.Delay(1000);

                Assert.False(connected);

                await server.StopAsync();
            }            
        }

        [Fact]
        public async Task TestConfigureSocketOptions() 
        {
            var connected = false;
            var s = default(Socket);

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .UseSessionHandler(async (s) =>
                {
                    connected = true;
                    await Task.CompletedTask;
                }, async (s, e) =>
                {
                    connected = false;                    
                    await Task.CompletedTask;
                })
                .ConfigureSocketOptions(socket =>
                {
                    s = socket;
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 7);
                })
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetDefaultServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                Assert.True(connected);

                Assert.Equal(10, (int)s.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime));
                Assert.Equal(5, (int)s.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval));
                Assert.Equal(7, (int)s.GetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount));

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                await Task.Delay(1000);

                Assert.False(connected);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestConsoleProtocol(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes("Hello World\r\n"));
                }).BuildAsServer() as IServer)
            {            
                Assert.True(await server.StartAsync());
                Assert.Equal(0, server.SessionCount);

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(hostConfigurator.GetServerEndPoint());                
                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("Hello World\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("Hello World", line);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCloseAfterSend(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes("Hello World\r\n"));
                    await s.CloseAsync(Channel.CloseReason.LocalClosing);
                }).BuildAsServer() as IServer)
            {            
                Assert.True(await server.StartAsync());
                Assert.Equal(0, server.SessionCount);

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(hostConfigurator.GetServerEndPoint());                
                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("Hello World\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("Hello World", line);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(UdpHostConfigurator))]
        public async Task TestMultipleHostStartup(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var hostBuilder = MultipleServerHostBuilder.Create()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddJsonFile("Config/multiple_server.json", optional: false, reloadOnChange: true);
                })
                .AddServer<TextPackageInfo, LinePipelineFilter>(builder =>
                {
                    hostConfigurator.Configure(builder);

                    builder
                        .ConfigureServerOptions((ctx, config) =>
                        {
                            return config.GetSection("TestServer1");
                        })
                        .UsePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                        {
                            await s.SendAsync(Utf8Encoding.GetBytes("Hello World\r\n"));
                        });
                });

            using(var host = hostBuilder.Build())
            {
                await host.StartAsync();
                await host.StopAsync();
            }
        }

        [Fact]
        [Trait("Category", "TestServiceProvider")]
        public async Task TestServiceProvider()
        {
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IHostConfigurator, RegularHostConfigurator>();
                }).BuildAsServer() as IServer)
            {            
                Assert.True(await server.StartAsync()); 

                Assert.IsType<RegularHostConfigurator>(server.ServiceProvider.GetService<IHostConfigurator>());
                
                await server.StopAsync();
            }
        }

        [Fact]
        public async Task TestStartWithDefaultConfig() 
        {
            var server = default(IServer);

            using (var host = SuperSocketHostBuilder.Create<TextPackageInfo, LinePipelineFilter>()
                .UseSessionHandler(s =>
                {
                    server = s.Server as IServer;
                    return new ValueTask();
                })
                .Build())
            {
                await host.StartAsync();

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetDefaultServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                Assert.Equal("TestServer", server.Name);

                Assert.Equal(1, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                await Task.Delay(1000);

                Assert.Equal(0, server.SessionCount);
                OutputHelper.WriteLine("SessionCount:" + server.SessionCount);

                await host.StopAsync();
            }
        }

        class SuperSocketServiceA : SuperSocketService<TextPackageInfo>
        {
            public SuperSocketServiceA(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions)
                : base(serviceProvider, serverOptions)
            {

            }
        }

        class SuperSocketServiceB : SuperSocketService<TextPackageInfo>
        {
            public SuperSocketServiceB(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions)
                : base(serviceProvider, serverOptions)
            {
                
            }
        }

        class MyTestService
        {
            public string Name { get; set; }

            public int Version { get; set; } = 0;

            public MyTestService()
            {

            }
        }

        class MyLocalTestService
        {
            public IServerInfo Server { get; private set; }

            public MyLocalTestService(IServerInfo server)
            {
                Server = server;
            }
        }

        [Fact]
        public async Task TestMultipleServerHost()
        {
            var serverName1 = "TestServer1";
            var serverName2 = "TestServer2";

            var server1 = default(IServer);
            var server2 = default(IServer);

            IHostEnvironment actualHostEvn = null;

            var hostBuilder = MultipleServerHostBuilder.Create()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    actualHostEvn = hostingContext.HostingEnvironment;
                    config.Sources.Clear();
                    config.AddJsonFile("Config/multiple_server.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddSingleton<MyTestService>();
                })
                .AddServer<SuperSocketServiceA, TextPackageInfo, LinePipelineFilter>(builder =>
                {
                    builder
                    .ConfigureServerOptions((ctx, config) =>
                    {
                        return config.GetSection(serverName1);
                    }).UseSessionHandler(async (s) =>
                    {
                        server1 = s.Server as IServer;
                        await s.SendAsync(Utf8Encoding.GetBytes($"{s.Server.Name}\r\n"));
                    })
                    .UseInProcSessionContainer()
                    .ConfigureServices((ctx, services) => services.AddSingleton<MyLocalTestService>());
                })
                .AddServer<SuperSocketServiceB, TextPackageInfo, LinePipelineFilter>(builder =>
                {
                    builder
                    .ConfigureServerOptions((ctx, config) =>
                    {
                        return config.GetSection(serverName2);
                    }).UseSessionHandler(async (s) =>
                    {
                        server2 = s.Server as IServer;
                        await s.SendAsync(Utf8Encoding.GetBytes($"{s.Server.Name}\r\n"));
                    })
                    .UseInProcSessionContainer()
                    .ConfigureServices((ctx, services) => services.AddSingleton<MyLocalTestService>());
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                });

            using(var host = hostBuilder.Build())
            {
                await host.StartAsync();

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetDefaultServerEndPoint());
                
                using (var stream = new NetworkStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal(serverName1, line);
                }
                
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetAlternativeServerEndPoint());
                
                using (var stream = new NetworkStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal(serverName2, line);
                }

                var hostEnv = server1.ServiceProvider.GetService<IHostEnvironment>();
                Assert.NotNull(hostEnv);
                Assert.Equal(actualHostEvn.ContentRootPath, hostEnv.ContentRootPath);

                var hostAppLifetime = server1.ServiceProvider.GetService<IHostApplicationLifetime>();
                Assert.NotNull(hostAppLifetime);
                
                var hostLifetime = server1.ServiceProvider.GetService<IHostLifetime>();
                Assert.NotNull(hostLifetime);

                var hostFromServices = server1.ServiceProvider.GetService<IHost>();
                Assert.NotNull(hostFromServices);

                Assert.NotSame(server1.GetSessionContainer(), server2.GetSessionContainer());

                var loggerFactory0 = host.Services.GetService<ILoggerFactory>();
                var loggerFactory1 = server1.ServiceProvider.GetService<ILoggerFactory>();
                var loggerFactory2 = server2.ServiceProvider.GetService<ILoggerFactory>();

                Assert.Equal(loggerFactory0, loggerFactory1);
                Assert.Equal(loggerFactory1, loggerFactory2);

                var testService0 = host.Services.GetService<MyTestService>();
                testService0.Name = "SameInstance";
                testService0.Version = 1;

                var testService1 = server1.ServiceProvider.GetService<MyTestService>();
                Assert.Equal(testService0.Name, testService1.Name);
                Assert.Equal(1, testService1.Version);
                testService1.Version = 2;
                Assert.Same(server1, server1.ServiceProvider.GetService<IServerInfo>());
                Assert.Same(server1, server1.ServiceProvider.GetService<MyLocalTestService>().Server);

                var testService2 = server2.ServiceProvider.GetService<MyTestService>();
                Assert.Equal(testService0.Name, testService2.Name);
                Assert.Equal(2, testService2.Version);
                Assert.Same(server2, server2.ServiceProvider.GetService<IServerInfo>());
                Assert.Same(server2, server2.ServiceProvider.GetService<MyLocalTestService>().Server);

                await host.StopAsync();
            }
        }
    }
}
