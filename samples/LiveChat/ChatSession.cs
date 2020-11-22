
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.WebSocket.Server;

namespace LiveChat
{
    public class ChatSession : WebSocketSession
    {
        public string Name { get; set; }
    }
}