using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Provides functionality to encode WebSocket packages with masking support.
    /// </summary>
    public class WebSocketMaskedEncoder : WebSocketEncoder
    {
        private static readonly Random _random = new Random();

        private const int MASK_LEN = 4;

        private const int MASK_OFFSET_RESET_THRESHOLD = 100000;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketMaskedEncoder"/> class with the specified buffer pool and fragment sizes.
        /// </summary>
        /// <param name="bufferPool">The buffer pool to use for encoding.</param>
        /// <param name="fragmentSizes">The sizes of data fragments for encoding.</param>
        public WebSocketMaskedEncoder(ArrayPool<byte> bufferPool, int[] fragmentSizes)
            : base(bufferPool, fragmentSizes)
        {
        }

        /// <summary>
        /// Creates a context for encoding data with masking support.
        /// </summary>
        /// <param name="writer">The buffer writer used for encoding.</param>
        /// <returns>An object representing the masking context.</returns>
        protected override object CreateDataEncodingContext(IBufferWriter<byte> writer)
        {
            var maskingContext = new MaskingContext();

            if (writer == null)
            {
                var maskBuffer = BufferPool.Rent(MASK_LEN);
                var mask = maskBuffer.AsMemory(0, MASK_LEN);
                GenerateMask(mask);
                maskingContext.Mask = mask;
                maskingContext.MaskBuffer = maskBuffer;
            }
            else
            {
                var mask = writer.GetMemory(MASK_LEN).Slice(0, MASK_LEN);
                GenerateMask(mask);
                maskingContext.Mask = mask;
            }

            return maskingContext;
        }

        /// <summary>
        /// Writes the head of a WebSocket frame with masking enabled.
        /// </summary>
        /// <param name="writer">The buffer writer to write the head to.</param>
        /// <param name="length">The length of the WebSocket frame.</param>
        /// <param name="headLen">The length of the head written.</param>
        /// <returns>A span representing the head of the WebSocket frame.</returns>
        protected override Span<byte> WriteHead(IBufferWriter<byte> writer, long length, out int headLen)
        {
            var head = base.WriteHead(writer, length, out headLen);            
            head[1] = (byte)(head[1] | 0x80);
            return head;
        }

        /// <summary>
        /// Handles the event when the head of the WebSocket frame is encoded with masking.
        /// </summary>
        /// <param name="writer">The buffer writer used for encoding.</param>
        /// <param name="encodingContext">The masking context.</param>
        protected override void OnHeadEncoded(IBufferWriter<byte> writer, object encodingContext)
        {
            var maskingContext = encodingContext as MaskingContext;

            if (maskingContext == null)
                return;

            // Means mask buffer was allocated from writter
            if (maskingContext.MaskBuffer == null)
            {
                writer.Advance(MASK_LEN);
            }
            else
            {
                writer.Write(maskingContext.Mask.Span);
            }
        }

        /// <summary>
        /// Handles the event when data is encoded with masking.
        /// </summary>
        /// <param name="encodedData">The encoded data.</param>
        /// <param name="encodingContext">The masking context.</param>
        /// <param name="previusEncodedDataSize">The size of previously encoded data.</param>
        protected override void OnDataEncoded(Span<byte> encodedData, object encodingContext, int previusEncodedDataSize)
        {
            var maskingContext = encodingContext as MaskingContext;
            MaskData(encodedData, maskingContext.Mask.Span, encodedData.Length, previusEncodedDataSize);
        }

        /// <summary>
        /// Encodes the body of a data message for a WebSocket package with masking.
        /// </summary>
        /// <param name="writer">The buffer writer to write the encoded data to.</param>
        /// <param name="pack">The WebSocket package containing the data message.</param>
        protected override void EncodeDataMessageBody(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            var mask = writer.GetMemory(MASK_LEN);
            GenerateMask(mask);
            writer.Advance(MASK_LEN);

            var maskOffset = 0;

            foreach (var dataPiece in pack.Data)
            {
                var data = dataPiece.Span;

                while (true)
                {
                    var destination = writer.GetSpan();
                    var writeLen = Math.Min(destination.Length, data.Length);

                    data[..writeLen].CopyTo(destination);
                    MaskData(destination, mask.Span, writeLen, maskOffset);
                    writer.Advance(writeLen);

                    maskOffset += writeLen;

                    if (maskOffset > MASK_OFFSET_RESET_THRESHOLD)
                    {
                        maskOffset %= MASK_LEN;
                    }

                    data = data[writeLen..];

                    if (data.Length == 0)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates a random mask for encoding WebSocket data.
        /// </summary>
        /// <param name="mask">The memory buffer to store the generated mask.</param>
        private void GenerateMask(Memory<byte> mask)
        {
            var span = mask.Span;

            for (var i = 0; i < MASK_LEN; i++)
            {
                span[i] = (byte)_random.Next(0, 255);
            }
        }

        /// <summary>
        /// Applies a mask to the specified data.
        /// </summary>
        /// <param name="data">The data to apply the mask to.</param>
        /// <param name="mask">The mask to apply.</param>
        /// <param name="dataLength">The length of the data to mask.</param>
        /// <param name="maskOffset">The offset to start applying the mask from.</param>
        private void MaskData(Span<byte> data, ReadOnlySpan<byte> mask, int dataLength, int maskOffset = 0)
        {
            for (var i = 0; i < dataLength; i++)
            {
                data[i] = (byte)(data[i] ^ mask[(i + maskOffset) % MASK_LEN]);
            }
        }

        /// <summary>
        /// Represents the context for masking WebSocket data.
        /// </summary>
        private class MaskingContext
        {
            /// <summary>
            /// Gets or sets the mask used for encoding.
            /// </summary>
            public ReadOnlyMemory<byte> Mask { get; set; }

            /// <summary>
            /// Gets or sets the buffer used to store the mask.
            /// </summary>
            public byte[] MaskBuffer { get; set; }
        }
    }
}