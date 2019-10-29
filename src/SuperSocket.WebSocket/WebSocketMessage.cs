using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public struct WebSocketMessage
    {
        public OpCode OpCode { get; set; }

        public string Message { get; set; }

        public ReadOnlySequence<byte> Data { get; set; }
    }
}