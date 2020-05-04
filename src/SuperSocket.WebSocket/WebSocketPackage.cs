using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public class WebSocketPackage
    {
        public OpCode OpCode { get; set; }

        internal byte OpCodeByte { get; set; }

        public bool FIN
        {
            get { return ((OpCodeByte & 0x80) == 0x80); }
        }

        public bool RSV1
        {
            get { return ((OpCodeByte & 0x40) == 0x40); }
        }

        public bool RSV2
        {
            get { return ((OpCodeByte & 0x20) == 0x20); }
        }

        public bool RSV3
        {
            get { return ((OpCodeByte & 0x10) == 0x10); }
        }

        internal bool HasMask { get; set; }

        internal long PayloadLength { get; set; }

        internal byte[] MaskKey { get; set; }

        public string Message { get; set; }

        public HttpHeader HttpHeader { get; set; }

        public ReadOnlySequence<byte> Data { get; set; }
    }
}
