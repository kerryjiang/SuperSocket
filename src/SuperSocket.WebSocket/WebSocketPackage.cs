using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents a WebSocket package and provides properties and methods for handling WebSocket frames.
    /// </summary>
    public class WebSocketPackage : IWebSocketFrameHeader
    {
        /// <summary>
        /// Gets or sets the operation code of the WebSocket frame.
        /// </summary>
        public OpCode OpCode { get; set; }

        /// <summary>
        /// Gets or sets the raw operation code byte of the WebSocket frame.
        /// </summary>
        internal byte OpCodeByte { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the FIN (final fragment) bit is set.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the RSV1 (reserved 1) bit is set.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the RSV2 (reserved 2) bit is set.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the RSV3 (reserved 3) bit is set.
        /// </summary>
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

        /// <summary>
        /// Saves the operation code byte by combining the reserved bits and the operation code.
        /// </summary>
        internal void SaveOpCodeByte()
        {
            OpCodeByte = (byte)((OpCodeByte & 0xF0) | (byte)OpCode);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the WebSocket frame has a mask.
        /// </summary>
        internal bool HasMask { get; set; }

        /// <summary>
        /// Gets or sets the payload length of the WebSocket frame.
        /// </summary>
        internal long PayloadLength { get; set; }

        /// <summary>
        /// Gets or sets the mask key used for decoding the WebSocket frame payload.
        /// </summary>
        internal byte[] MaskKey { get; set; }

        /// <summary>
        /// Gets or sets the message content of the WebSocket frame.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the HTTP header associated with the WebSocket frame.
        /// </summary>
        public HttpHeader HttpHeader { get; set; }

        /// <summary>
        /// Gets or sets the payload data of the WebSocket frame.
        /// </summary>
        public ReadOnlySequence<byte> Data { get; set; }

        /// <summary>
        /// Gets or sets the head segment of the payload data sequence.
        /// </summary>
        internal SequenceSegment Head { get; set; }

        /// <summary>
        /// Gets or sets the tail segment of the payload data sequence.
        /// </summary>
        internal SequenceSegment Tail { get; set; }

        /// <summary>
        /// Concatenates the current payload data sequence with another sequence.
        /// </summary>
        /// <param name="second">The sequence to concatenate with the current sequence.</param>
        internal void ConcatSequence(ref ReadOnlySequence<byte> second)
        {
            if (Head == null)
            {
                (Head, Tail) = second.DestructSequence();
                return;
            }

            if (!second.IsEmpty)
            {
                foreach (var segment in second)
                {
                    Tail = Tail.SetNext(new SequenceSegment(segment));
                }
            }
        }

        /// <summary>
        /// Builds the payload data sequence from the head and tail segments.
        /// </summary>
        internal void BuildData()
        {
            Data = new ReadOnlySequence<byte>(Head, 0, Tail, Tail.Memory.Length);
        }
    }
}
