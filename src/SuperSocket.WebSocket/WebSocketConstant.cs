using System;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Provides constants used in WebSocket communication.
    /// </summary>
    public class WebSocketConstant
    {
        /// <summary>
        /// The HTTP header for the host.
        /// </summary>
        public const string Host = "Host";

        /// <summary>
        /// The HTTP header for the connection type.
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        /// The WebSocket key 1 header.
        /// </summary>
        public const string SecWebSocketKey1 = "Sec-WebSocket-Key1";

        /// <summary>
        /// The WebSocket key 2 header.
        /// </summary>
        public const string SecWebSocketKey2 = "Sec-WebSocket-Key2";

        /// <summary>
        /// The WebSocket key 3 header.
        /// </summary>
        public const string SecWebSocketKey3 = "Sec-WebSocket-Key3";

        /// <summary>
        /// The WebSocket key header.
        /// </summary>
        public const string SecWebSocketKey = "Sec-WebSocket-Key";

        /// <summary>
        /// The WebSocket version header.
        /// </summary>
        public const string SecWebSocketVersion = "Sec-WebSocket-Version";

        /// <summary>
        /// The WebSocket protocol header.
        /// </summary>
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";

        /// <summary>
        /// The WebSocket origin header.
        /// </summary>
        public const string SecWebSocketOrigin = "Sec-WebSocket-Origin";

        /// <summary>
        /// The WebSocket extensions header.
        /// </summary>
        public const string SecWebSocketExtensions = "Sec-WebSocket-Extensions";

        /// <summary>
        /// The prefix for the WebSocket response extensions line.
        /// </summary>
        public const string ResponseExtensionsLinePrefix = SecWebSocketExtensions + ":";

        /// <summary>
        /// The WebSocket magic string used in the handshake.
        /// </summary>
        public const string Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// The HTTP cookie header.
        /// </summary>
        public const string Cookie = "Cookie";

        /// <summary>
        /// The HTTP upgrade header.
        /// </summary>
        public const string Upgrade = "Upgrade";

        /// <summary>
        /// The HTTP origin header.
        /// </summary>
        public const string Origin = "Origin";

        /// <summary>
        /// The WebSocket response head line for protocol handshake (version 0.0).
        /// </summary>
        public const string ResponseHeadLine00 = "HTTP/1.1 101 WebSocket Protocol Handshake\r\n";

        /// <summary>
        /// The WebSocket response head line for switching protocols (version 1.0).
        /// </summary>
        public const string ResponseHeadLine10 = "HTTP/1.1 101 Switching Protocols\r\n";

        /// <summary>
        /// The WebSocket response upgrade line.
        /// </summary>
        public const string ResponseUpgradeLine = Upgrade + ": WebSocket\r\n";

        /// <summary>
        /// The WebSocket response connection line.
        /// </summary>
        public const string ResponseConnectionLine = Connection + ": Upgrade\r\n";

        /// <summary>
        /// The WebSocket response origin line format.
        /// </summary>
        public const string ResponseOriginLine = SecWebSocketOrigin + ": {0}\r\n";

        /// <summary>
        /// The WebSocket response location line format.
        /// </summary>
        public const string ResponseLocationLine = "Sec-WebSocket-Location: {0}://{1}{2}\r\n";

        /// <summary>
        /// The WebSocket response protocol line format.
        /// </summary>
        public const string ResponseProtocolLine = SecWebSocketProtocol + ": {0}\r\n";

        /// <summary>
        /// The WebSocket response accept line format.
        /// </summary>
        public const string ResponseAcceptLine = "Sec-WebSocket-Accept: {0}\r\n";

        /// <summary>
        /// The start byte for WebSocket frames.
        /// </summary>
        public const byte StartByte = 0x00;

        /// <summary>
        /// The end byte for WebSocket frames.
        /// </summary>
        public const byte EndByte = 0xFF;

        /// <summary>
        /// The closing handshake bytes for WebSocket.
        /// </summary>
        public static byte[] ClosingHandshake = new byte[] { 0xFF, 0x00 };

        /// <summary>
        /// The schema for unencrypted WebSocket connections.
        /// </summary>
        public const string WsSchema = "ws";

        /// <summary>
        /// The schema for encrypted WebSocket connections.
        /// </summary>
        public const string WssSchema = "wss";
    }
}
