using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.WebSocket.Protocol
{
    class WebSocketSecKey3ReceiveFilter : WebSocketReceiveFilterBase
    {
        public WebSocketSecKey3ReceiveFilter(WebSocketReceiveFilterBase prevFilter)
            : base(prevFilter)
        {
            
        }

        public override IWebSocketFragment Filter(byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int rest)
        {
            var webSocketSession = Session;

            int total = BufferSegments.Count + length;

            if (total == SecKey3Len)
            {
                byte[] key = new byte[SecKey3Len];
                BufferSegments.CopyTo(key);
                Array.Copy(readBuffer, offset, key, BufferSegments.Count, length);
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key;
                BufferSegments.ClearSegements();
                rest = 0;
                if(Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession))
                    return HandshakeRequestInfo;
            }
            else if (total > SecKey3Len)
            {
                byte[] key = new byte[8];
                BufferSegments.CopyTo(key);
                Array.Copy(readBuffer, offset, key, BufferSegments.Count, SecKey3Len - BufferSegments.Count);
                webSocketSession.Items[WebSocketConstant.SecWebSocketKey3] = key;
                BufferSegments.ClearSegements();
                rest = total - SecKey3Len;
                if(Handshake(webSocketSession.AppServer.WebSocketProtocolProcessor, webSocketSession))
                    return HandshakeRequestInfo;
            }
            else
            {
                AddArraySegment(readBuffer, offset, length, isReusableBuffer);
                rest = 0;
                NextReceiveFilter = this;
                return null;
            }

            return null;
        }
    }
}
