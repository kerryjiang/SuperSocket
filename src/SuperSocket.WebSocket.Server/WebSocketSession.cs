using System;
using System.Buffers;
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

        internal bool InClosing { get; set; }

        private static readonly IPackageEncoder<WebSocketMessage> _messageEncoder = new WebSocketEncoder();

        public ValueTask SendAsync(WebSocketMessage message)
        {
            return this.Channel.SendAsync(_messageEncoder, message);
        }

        public ValueTask SendAsync(string message)
        {
            return SendAsync(new WebSocketMessage
            {
                OpCode = OpCode.Text,
                Message = message,
            });
        }

        public ValueTask CloseAsync(CloseReason reason)
        {
            var closeReasonCode = (short)reason;

            return SendAsync(new WebSocketMessage
            {
                OpCode = OpCode.Close,
                Data = new ReadOnlySequence<byte>(new byte[]
                {
                    (byte) (closeReasonCode / 256),
                    (byte) (closeReasonCode % 256)
                })
            });
        }
    }
}