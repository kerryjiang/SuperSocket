using System.Buffers;
using System.Net.WebSockets;
using System.Text;

var client = new ClientWebSocket();

await client.ConnectAsync(new Uri("ws://127.0.0.1:4040"), CancellationToken.None);

var sendData = Encoding.UTF8.GetBytes(ClientTest.Properties.Resources.大json_2_);

Console.WriteLine($"发送数据：{sendData.Length}");

await client.SendAsync(
    endOfMessage: true,
    buffer: new ArraySegment<byte>(sendData),
    messageType: WebSocketMessageType.Text,//只有Text模式会出现
    cancellationToken: CancellationToken.None);

Console.WriteLine($"正在接受...");

while (true)
{
    var receiveBuffer = ArrayPool<byte>.Shared.Rent(sendData.Length);

    try
    {
        var webSocketReceiveResult = await client.ReceiveAsync(receiveBuffer.AsMemory(), CancellationToken.None);

        var result = Encoding.UTF8.GetString(receiveBuffer, 0, webSocketReceiveResult.Count);

        Console.WriteLine($"接受长度：{webSocketReceiveResult.Count}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        break;
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(receiveBuffer);
    }
}

Console.ReadKey();