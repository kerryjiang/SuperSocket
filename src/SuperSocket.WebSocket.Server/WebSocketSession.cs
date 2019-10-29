using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketSession : AppSession
    {
        public bool Handshaked { get; internal set; }

        public HttpHeader HttpHeader { get; internal set; }

        private static readonly IPackageEncoder<WebSocketMessage> _messageEncoder = new WebSocketEncoder();

        public ValueTask SendAsync(WebSocketMessage message)
        {
            return this.Channel.SendAsync(_messageEncoder, message);
        }
    }
}