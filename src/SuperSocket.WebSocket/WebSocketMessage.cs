using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public struct WebSocketMessage
    {
        public OpCode OpCode { get; set; }

        internal bool RSV1 { get; set; }

        internal bool RSV2 { get; set; }

        internal bool RSV3 { get; set; }

        internal byte GetOpCodeByte()
        {
            var code = (byte)OpCode;

            if (RSV1)
                code = (byte)(code | 0x40);

            if (RSV2)
                code = (byte)(code | 0x20);

            if (RSV3)
                code = (byte)(code | 0x10);

            return code;
        }

        internal bool IsCompressed { get; set; }

        public string Message { get; set; }

        public ReadOnlySequence<byte> Data { get; set; }
    }
}