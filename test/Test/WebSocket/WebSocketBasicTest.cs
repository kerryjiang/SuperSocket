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
using Microsoft.Extensions.Hosting;
using System.Security.Authentication;
using SuperSocket;
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

            using (var server = CreateWebSocketServerBuilder(hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:4040"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        /*
        [Fact]
        [Trait("Category", "WebSocketHandshakeTimeOut")]
        public async Task TestHandshakeTimeOut() 
        {
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
            }).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                var endPoint = new IPEndPoint(IPAddress.Loopback, 4040);
                await socket.ConnectAsync(endPoint);
                Assert.True(socket.Connected);
                await Task.Delay(1000 * 5);

                Assert.False(IsConnected(socket));

                Assert.Equal(0, socket.Send(new byte[1024]));

                await server.StopAsync();
            }
        }
        */


        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestMessageSendReceive(Type hostConfiguratorType) 
        {
            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.ConfigureWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(message.Message);
                });
            }, CreateObject<IHostConfigurator>(hostConfiguratorType)).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                await websocket.ConnectAsync(new Uri("ws://localhost:4040"), CancellationToken.None);

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
    }
}
