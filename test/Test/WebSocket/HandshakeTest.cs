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
using System.Net.WebSockets;

namespace Tests.WebSocket
{
    [Collection("WebSocket")]
    public class HandshakeTest : WebSocketServerTestBase
    {
        public HandshakeTest(ITestOutputHelper outputHelper)
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
    }
}
