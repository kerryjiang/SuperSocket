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
using SuperSocket;
using System.Linq;
using System.Reflection;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Host;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace SuperSocket.Tests
{
    [Trait("Category", "Basic")]
    public class SessionTest : TestClassBase
    {
        public SessionTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
        }

        [Fact]
        public async Task TestSessionEvents() 
        {
            var hostConfigurator = new RegularHostConfigurator();
            var connected = false;
            IAppSession session = null;

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UseSessionHandler((s) =>
                {
                    connected = true;
                    session = s;
                    return new ValueTask();
                })
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(hostConfigurator.GetServerEndPoint());
                OutputHelper.WriteLine("Connected.");                

                await Task.Delay(1000);

                session.Closed += (s, e) =>
                {
                    connected = false;
                    session.DataContext = "Hello, my love!";
                    return new ValueTask();
                };

                session.Connected += async (s, e) =>
                {
                    OutputHelper.WriteLine("Session.Connected event was triggered");
                    await Task.CompletedTask;
                };

                var itemKey = "GirlFriend";
                var itemValue = "Who?";

                session[itemKey] = itemValue;

                Assert.Equal(SessionState.Connected, session.State);
                Assert.True(connected);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                await Task.Delay(1000);

                Assert.Equal(SessionState.Closed, session.State);
                Assert.False(connected);
                Assert.Equal(itemValue, session[itemKey]);

                Assert.Equal(1, GetEventInvocationCount(session, nameof(session.Closed)));
                Assert.Equal(1, GetEventInvocationCount(session, nameof(session.Connected)));

                session.Reset();

                Assert.Null(session.Connection);
                Assert.Null(session.DataContext);
                Assert.Equal(SessionState.None, session.State);
                Assert.Null(session[itemKey]);
                Assert.Equal(0, GetEventInvocationCount(session, nameof(session.Closed)));
                Assert.Equal(0, GetEventInvocationCount(session, nameof(session.Connected)));

                await server.StopAsync();
            }            
        }

        private int GetEventInvocationCount(object objectWithEvent, string eventName)
        {
            var allBindings = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;            
            var type = objectWithEvent.GetType();
            var fieldInfo = type.GetField(eventName, allBindings);
            var value = fieldInfo.GetValue(objectWithEvent);

            var handler = value as Delegate;

            if (handler == null)
                return 0;

            return handler.GetInvocationList().Length;
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(KestralConnectionHostConfigurator))]
        public async Task TestCloseReason(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            IAppSession session = null;

            CloseReason closeReason = CloseReason.Unknown;

            var connectEvent = new AutoResetEvent(false);
            var closeEvent = new AutoResetEvent(false);

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .ConfigureAppConfiguration((HostBuilder, configBuilder) =>
                    {
                        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "serverOptions:maxPackageLength", "8" }
                        });
                    })
                .UseSessionHandler(
                    onConnected: (s) =>
                        {
                            session = s;
                            connectEvent.Set();
                            return ValueTask.CompletedTask;
                        },
                    onClosed: (s, e) =>
                        {
                            closeReason = e.Reason;
                            closeEvent.Set();
                            return ValueTask.CompletedTask;
                        })
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                // RemoteClosing
                using (var socket = CreateClient(hostConfigurator))
                using (var socketStream = await hostConfigurator.GetClientStream(socket))
                {
                    OutputHelper.WriteLine("Connected.");

                    connectEvent.WaitOne(1000);

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

                    closeEvent.WaitOne(1000);

                    Assert.Equal(SessionState.Closed, session.State);
                    Assert.Equal(CloseReason.RemoteClosing, closeReason);
                }

                using (var socket = CreateClient(hostConfigurator))
                using (var socketStream = await hostConfigurator.GetClientStream(socket))
                {
                    connectEvent.WaitOne(1000);

                    // LocalClosing
                    closeReason = CloseReason.Unknown;

                    await session.CloseAsync(CloseReason.LocalClosing);

                    closeEvent.WaitOne(1000);

                    Assert.Equal(SessionState.Closed, session.State);
                    Assert.Equal(CloseReason.LocalClosing, closeReason);
                }

                using (var socket = CreateClient(hostConfigurator))
                using (var socketStream = await hostConfigurator.GetClientStream(socket))
                {
                    connectEvent.WaitOne(1000);

                    // ProtocolError
                    closeReason = CloseReason.Unknown;

                    var buffer = Encoding.ASCII.GetBytes("123456789\r\n"); // package size exceeds maxPackageSize(8)
                    socketStream.Write(buffer, 0, buffer.Length);
                    socketStream.Flush();

                    closeEvent.WaitOne(1000);

                    Assert.Equal(SessionState.Closed, session.State);
                    Assert.Equal(CloseReason.ProtocolError, closeReason);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator), 198)]
        [InlineData(typeof(RegularHostConfigurator), 255)]
        public async Task TestCustomCloseReason(Type hostConfiguratorType, int closeReasonCode)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            IAppSession session = null;

            CloseReason closeReason = CloseReason.Unknown;

            var resetEvent = new AutoResetEvent(false);

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .ConfigureAppConfiguration((HostBuilder, configBuilder) =>
                    {
                        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "serverOptions:maxPackageLength", "8" }
                        });
                    })
                .UseSessionHandler(
                    onConnected: (s) =>
                        {
                            session = s;
                            resetEvent.Set();
                            return ValueTask.CompletedTask;
                        },
                    onClosed: (s, e) =>
                        {
                            closeReason = e.Reason;
                            resetEvent.Set();
                            return ValueTask.CompletedTask;
                        })
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                using (var socket = CreateClient(hostConfigurator))
                using (var socketStream = await hostConfigurator.GetClientStream(socket))
                {
                    Assert.True(resetEvent.WaitOne(1000));

                    // LocalClosing
                    closeReason = CloseReason.Unknown;

                    var closeReasonRequested = (CloseReason)closeReasonCode;

                    await session.CloseAsync(closeReasonRequested);

                    Assert.True(resetEvent.WaitOne(1000));

                    Assert.Equal(SessionState.Closed, session.State);
                    Assert.Equal(closeReasonRequested, closeReason);
                }

                await server.StopAsync();
            }
        }

        [Fact]
        public async Task TestConsoleProtocol() 
        {
            var hostConfigurator = new RegularHostConfigurator();
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

        [Fact]
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
    }
}
