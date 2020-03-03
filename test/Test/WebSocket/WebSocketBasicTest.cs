using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.WebSocket.Server;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.WebSocket
{
    [Trait("Category", "WebSocketBasic")]
    public class WebSocketBasicTest : WebSocketServerTestBase
    {
        private Encoding _encoding = new UTF8Encoding(false);

        public WebSocketBasicTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestHandshake(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var serverSessionPath = string.Empty;

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                builder.ConfigureSessionHandler((s) =>
                {
                    serverSessionPath = (s as WebSocketSession).Path;
                    return new ValueTask();
                });

                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var path = "/app/talk";

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:4040" + path), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                // test path
                Assert.Equal(path, serverSessionPath);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }


        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestLongMessageFromServer(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var serverSessionPath = string.Empty;

            var longText = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                builder.ConfigureSessionHandler((s) =>
                {
                    serverSessionPath = (s as WebSocketSession).Path;
                    (s as WebSocketSession).SendAsync(longText);
                    return new ValueTask();
                });

                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var path = "/app/talk";

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:4040" + path), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var receiveBuffer = new byte[1024 * 1024 * 4];
                
                var receiveSegment = new ArraySegment<byte>(receiveBuffer, 0, receiveBuffer.Length);
                var result = await websocket.ReceiveAsync(receiveSegment, CancellationToken.None);

                Assert.Equal(WebSocketMessageType.Text, result.MessageType);

                var receivedMessage = _encoding.GetString(receiveSegment.Array, 0, result.Count);

                Assert.Equal(longText, receivedMessage);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        
        [Trait("Category", "WebSocketHandshakeTimeOut")]
        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestHandshakeTimeOut(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                builder.ConfigureServices((ctx, services) =>
                {
                    services.Configure<HandshakeOptions>(options =>
                    {
                        options.CheckingInterval = 1;
                        options.OpenHandshakeTimeOut = 1;
                    });
                });
                return builder;
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;
                var endPoint = new IPEndPoint(IPAddress.Loopback, 4040);
                await socket.ConnectAsync(endPoint);                
                Assert.True(socket.Connected);
                await Task.Delay(1000 * 5);
                //Assert.False(socket.Connected);
                await server.StopAsync();
            }
        }        


        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestMessageSendReceive(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.ConfigureWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(message.Message);
                });
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:4040"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var receiveBuffer = new byte[256];

                for (var i = 0; i < 100; i++)
                {
                    var message = Guid.NewGuid().ToString();
                    var data = _encoding.GetBytes(message);
                    var segment = new ArraySegment<byte>(data, 0, data.Length);

                    await websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    
                    var receiveSegment = new ArraySegment<byte>(receiveBuffer, 0, receiveBuffer.Length);
                    var result = await websocket.ReceiveAsync(receiveSegment, CancellationToken.None);

                    Assert.Equal(WebSocketMessageType.Text, result.MessageType);

                    var receivedMessage = _encoding.GetString(receiveSegment.Array, 0, result.Count);

                    OutputHelper.WriteLine(receivedMessage);

                    Assert.Equal(message, receivedMessage);
                }

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommands(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder
                    .UseCommand<StringPackageInfo, StringPackageConverter>(commandOptions =>
                    {
                        // register commands one by one
                        commandOptions.AddCommand<ADD>();
                        commandOptions.AddCommand<MULT>();
                        commandOptions.AddCommand<SUB>();
                        // register all commands in one aassembly
                        //commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
                    });
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:4040"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var receiveBuffer = new byte[256];

                Assert.Equal("11", await GetWebSocketReply(websocket, receiveBuffer, "ADD 5 6"));
                Assert.Equal("8", await GetWebSocketReply(websocket, receiveBuffer, "SUB 10 2"));
                Assert.Equal("21", await GetWebSocketReply(websocket, receiveBuffer, "MULT 3 7"));

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        private async ValueTask<string> GetWebSocketReply(ClientWebSocket websocket, byte[] receiveBuffer, string request)
        {
            var data = _encoding.GetBytes(request);
            var segment = new ArraySegment<byte>(data, 0, data.Length);

            await websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            
            var receiveSegment = new ArraySegment<byte>(receiveBuffer, 0, receiveBuffer.Length);
            var result = await websocket.ReceiveAsync(receiveSegment, CancellationToken.None);

            Assert.Equal(WebSocketMessageType.Text, result.MessageType);

            return _encoding.GetString(receiveBuffer, 0, result.Count);
        }

        class ADD : IAsyncCommand<WebSocketSession, StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(WebSocketSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Sum();

                await session.SendAsync(result.ToString());
            }
        }

        class MULT : IAsyncCommand<WebSocketSession, StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(WebSocketSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x * y);

                await session.SendAsync(result.ToString());
            }
        }

        class SUB : IAsyncCommand<WebSocketSession, StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(WebSocketSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x - y);

                await session.SendAsync(result.ToString());
            }
        }
    }
}
