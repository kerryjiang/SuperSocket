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
using SuperSocket;
using SuperSocket.WebSocket.Server;
using System.Net.WebSockets;

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

        [Fact]
        public async Task TestHandshake() 
        {
            using (var server = CreateWebSocketServerBuilder()
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                await websocket.ConnectAsync(new Uri("ws://localhost:4040"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);
            }
        }

        [Fact]
        public async Task TestMessageSendReceive() 
        {
            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.ConfigureWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(message.Message);
                });
            })
                .BuildAsServer())
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
            }
        }
    }
}
