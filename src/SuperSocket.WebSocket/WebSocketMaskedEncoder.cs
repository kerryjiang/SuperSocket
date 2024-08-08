using System;
using System.Buffers;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketMaskedEncoder : WebSocketEncoder
    {
        private static readonly Random _random = new Random();

        private const int MASK_LEN = 4;

        private const int MASK_OFFSET_RESET_THRESHOLD = 100000;

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

        protected override void OnHeadEncoded(IBufferWriter<byte> writer, object encodingContext)
        {
            var maskingContext = encodingContext as MaskingContext;

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

        protected override void OnDataEncoded(Span<byte> encodedData, object encodingContext, int previusEncodedDataSize)
        {
            var maskingContext = encodingContext as MaskingContext;
            MaskData(encodedData, maskingContext.Mask.Span, encodedData.Length, previusEncodedDataSize);
        }

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

        private void GenerateMask(Memory<byte> mask)
        {
            var span = mask.Span;

            for (var i = 0; i < MASK_LEN; i++)
            {
                span[i] = (byte)_random.Next(0, 255);
            }
        }

        private void MaskData(Span<byte> data, ReadOnlySpan<byte> mask, int dataLength, int maskOffset = 0)
        {
            for (var i = 0; i < dataLength; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i + maskOffset % MASK_LEN]);
            }
        }

        private class MaskingContext
        {
            public ReadOnlyMemory<byte> Mask { get; set; }

            public byte[] MaskBuffer { get; set; }
        }
    }
}