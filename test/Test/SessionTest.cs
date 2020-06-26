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

namespace Tests
{
    [Trait("Category", "Basic")]
    public class SessionTest : TestClassBase
    {
        public SessionTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
        }

        [Fact]
        [Trait("Category", "TestSessionEvents")]
        public async Task TestSessionEvents() 
        {
            var connected = false;
            IAppSession session = null;

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
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
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");                

                await Task.Delay(1000);

                session.Closed += (s, e) =>
                {
                    connected = false;
                    session.DataContext = "Hello, my love!";
                    return new ValueTask();
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

                session.Reset();

                Assert.Null(session.Channel);
                Assert.Null(session.DataContext);
                Assert.Equal(SessionState.None, session.State);
                Assert.Null(session[itemKey]);
                Assert.Equal(0, GetEventInvocationCount(session, nameof(session.Closed)));

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

        [Fact]
        public async Task TestConsoleProtocol() 
        {
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .UsePackageHandler(async (IAppSession s, TextPackageInfo p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes("Hello World\r\n"));
                }).BuildAsServer() as IServer)
            {            
                Assert.True(await server.StartAsync());
                Assert.Equal(0, server.SessionCount);

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                
                using (var stream = new NetworkStream(client))
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
