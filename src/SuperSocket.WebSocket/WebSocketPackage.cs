using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public class WebSocketPackage : IWebSocketFrameHeader
    {
        public OpCode OpCode { get; set; }

        internal byte OpCodeByte { get; set; }

        public bool FIN
        {
            get { return ((OpCodeByte & 0x80) == 0x80); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x80);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x80);
            }
        }

        public bool RSV1
        {
            get { return ((OpCodeByte & 0x40) == 0x40); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x40);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x40);
            }
        }

        public bool RSV2
        {
            get { return ((OpCodeByte & 0x20) == 0x20); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x20);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x20);
            }
        }

        public bool RSV3
        {
            get { return ((OpCodeByte & 0x10) == 0x10); }
            set
            {
                if (value)
                    OpCodeByte = (byte)(OpCodeByte | 0x10);
                else
                    OpCodeByte = (byte)(OpCodeByte ^ 0x10);
            }
        }

        internal void LoadOpCodeByte()
        {
            OpCode = (OpCode)(OpCodeByte & 0b_1111_0000);
        }

        internal void SaveOpCodeByte()
        {
            OpCodeByte = (byte)(OpCodeByte | (byte)OpCode);
        }

        internal bool HasMask { get; set; }

        internal long PayloadLength { get; set; }

        internal byte[] MaskKey { get; set; }

        public string Message { get; set; }

        public HttpHeader HttpHeader { get; set; }

        public ReadOnlySequence<byte> Data { get; set; }
    }
}
