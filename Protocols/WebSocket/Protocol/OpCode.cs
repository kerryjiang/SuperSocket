using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol
{
    class OpCode
    {
        public const sbyte Plain = -2; // defined by SuperWebSocket, to support hybi-00
        public const string PlainTag = "-2";

        public const sbyte Handshake = -1; // defined by SuperWebSocket
        public const string HandshakeTag = "-1";

        public const sbyte Continuation = 0;
        public const string ContinuationTag = "0";

        public const sbyte Text = 1;
        public const string TextTag = "1";

        public const sbyte Binary = 2;
        public const string BinaryTag = "2";

        public const sbyte Close = 8;
        public const string CloseTag = "8";

        public const sbyte Ping = 9;
        public const string PingTag = "9";

        public const sbyte Pong = 10;
        public const string PongTag = "10";
    }
}
