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

namespace Tests
{
    [Trait("Category", "Basic")]
    public class MainTest : TestClassBase
    {
        public MainTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
        }

        [Fact]
        [Trait("Category", "TestSessionCount")]
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
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
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
        [Trait("Category", "TestSessionHandlers")]
        public async Task TestSessionHandlers() 
        {
            var connected = false;

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>()
                .UseSessionHandler((s) =>
                {
                    connected = true;
                    return new ValueTask();
                }, (s) =>
                {
                    connected = false;
                    return new ValueTask();
                }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                Assert.True(connected);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                await Task.Delay(1000);

                Assert.False(connected);

                await server.StopAsync();
            }            
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
