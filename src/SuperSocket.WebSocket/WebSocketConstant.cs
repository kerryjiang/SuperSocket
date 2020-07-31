using System;

namespace SuperSocket.WebSocket
{
    public class WebSocketConstant
    {
        public const string Host = "Host";
        public const string Connection = "Connection";
        public const string SecWebSocketKey1 = "Sec-WebSocket-Key1";
        public const string SecWebSocketKey2 = "Sec-WebSocket-Key2";
        public const string SecWebSocketKey3 = "Sec-WebSocket-Key3";
        public const string SecWebSocketKey = "Sec-WebSocket-Key";
        public const string SecWebSocketVersion = "Sec-WebSocket-Version";
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
        public const string SecWebSocketOrigin = "Sec-WebSocket-Origin";
        public const string SecWebSocketExtensions = "Sec-WebSocket-Extensions";
        public const string ResponseExtensionsLinePrefix = SecWebSocketExtensions + ":";
        public const string Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        public const string Cookie = "Cookie";
        public const string Upgrade = "Upgrade";
        public const string Origin = "Origin";
        public const string ResponseHeadLine00 = "HTTP/1.1 101 WebSocket Protocol Handshake\r\n";
        public const string ResponseHeadLine10 = "HTTP/1.1 101 Switching Protocols\r\n";
        public const string ResponseUpgradeLine = Upgrade + ": WebSocket\r\n";
        public const string ResponseConnectionLine = Connection + ": Upgrade\r\n";
        public const string ResponseOriginLine = SecWebSocketOrigin + ": {0}\r\n";
        public const string ResponseLocationLine = "Sec-WebSocket-Location: {0}://{1}{2}\r\n";
        public const string ResponseProtocolLine = SecWebSocketProtocol + ": {0}\r\n";
        public const string ResponseAcceptLine = "Sec-WebSocket-Accept: {0}\r\n";
        public const byte StartByte = 0x00;
        public const byte EndByte = 0xFF;
        public static byte[] ClosingHandshake = new byte[] { 0xFF, 0x00 };
        public const string WsSchema = "ws";
        public const string WssSchema = "wss";
    }
}
