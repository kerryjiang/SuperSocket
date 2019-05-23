using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public class WebSocketPackage
    {
        public OpCode OpCode { get; set; }

        public string Message { get; set; }

        public HttpHeader HttpHeader { get; set; }

        public ReadOnlySequence<byte> Data { get; set; }
    }
}
