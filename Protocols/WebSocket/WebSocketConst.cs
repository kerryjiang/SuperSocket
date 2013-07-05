using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket
{
    class WebSocketConstant
    {
        public const string Host = "Host";
        public const string Connection = "Connection";
        public const string SecWebSocketKey1 = "Sec-WebSocket-Key1";
        public const string SecWebSocketKey2 = "Sec-WebSocket-Key2";
        public const string SecWebSocketKey3 = "Sec-WebSocket-Key3";
        public const string SecWebSocketKey = "Sec-WebSocket-Key";
        public const string SecWebSocketVersion = "Sec-WebSocket-Version";
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
        public const string WebSocketProtocol = "WebSocket-Protocol";
        public const string Cookie = "Cookie";
        public const string Upgrade = "Upgrade";
        public const string Origin = "Origin";
        public const string ResponseHeadLine00 = "HTTP/1.1 101 WebSocket Protocol Handshake";
        public const string ResponseHeadLine10 = "HTTP/1.1 101 Switching Protocols";
        public const string ResponseUpgradeLine = Upgrade + ": WebSocket";
        public const string ResponseConnectionLine = Connection + ": Upgrade";
        public const string ResponseOriginLine = "Sec-WebSocket-Origin: {0}";
        public const string ResponseLocationLine = "Sec-WebSocket-Location: {0}://{1}{2}";
        public const string ResponseProtocolLine = SecWebSocketProtocol + ": {0}";
        public const string ResponseAcceptLine = "Sec-WebSocket-Accept: {0}";
        public const byte StartByte = 0x00;
        public const byte EndByte = 0xFF;
        public static byte[] ClosingHandshake = new byte[] { 0xFF, 0x00 };
        public const string WsSchema = "ws";
        public const string WssSchema = "wss";
    }
}
