using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket
{
    public class WebSocketEncoder : IPackageEncoder<WebSocketPackage>
    {
        private static readonly Encoding _textEncoding = new UTF8Encoding(false);
        private const int _size0 = 126;
        private const int _size1 = 65536;
        public IReadOnlyList<IWebSocketExtension> Extensions { get; set; }

        private int WriteHead(ref Span<byte> head, byte opCode, long length)
        {
            head[0] = opCode;

            if (length < _size0)
            {
                head[1] = (byte)length;
                return 2;
            }
            else if (length < _size1)
            {
                head[1] = (byte)_size0;
                head[2] = (byte)(length / 256);
                head[3] = (byte)(length % 256);
                return 4;
            }
            else
            {
                head[1] = (byte)127;

                long left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    if (left == 0)
                    {
                        head[i] = 0;
                        continue;
                    }

                    head[i] = (byte)(left % unit);
                    left = left / unit;
                }

                return 10;
            }
        }

        private int EncodeEmptyFragment(IBufferWriter<byte> writer, byte opCode, int expectedHeadLength)
        {
            return EncodeSingleFragment(writer, opCode, expectedHeadLength, default);
        }

        private int EncodeFragment(IBufferWriter<byte> writer, byte opCode, int expectedHeadLength, int fragmentSize, ReadOnlySpan<char> text, Encoder encoder, out int charsUsed)
        {
            charsUsed = 0;

            var head = writer.GetSpan(expectedHeadLength);

            writer.Advance(expectedHeadLength);

            var buffer = writer.GetSpan(fragmentSize).Slice(0, fragmentSize);
            
            encoder.Convert(text, buffer, false, out charsUsed, out int bytesUsed, out bool completed);
            writer.Advance(bytesUsed);

            var totalBytes = bytesUsed;
            var isFinal = completed && text.Length == charsUsed;

            if (isFinal)
                opCode = (byte)(opCode | 0x80);

            WriteHead(ref head, opCode, totalBytes);

            return totalBytes + expectedHeadLength;
        }

        private int EncodeFragmentWithBuffer(IBufferWriter<byte> writer, byte opCode, int fragmentSize, ReadOnlySpan<char> text, Encoder encoder, out int charsUsed)
        {
            charsUsed = 0;

            var bufferPool = ArrayPool<byte>.Shared;
            var buffer = bufferPool.Rent(fragmentSize);

            try
            {
                var bufferSpan = buffer.AsSpan().Slice(0, fragmentSize);
                
                encoder.Convert(text, bufferSpan, false, out charsUsed, out int bytesUsed, out bool completed);

                var totalBytes = bytesUsed;
                var isFinal = completed && text.Length == charsUsed;

                if (isFinal)
                    opCode = (byte)(opCode | 0x80);

                var headLen = bytesUsed < _size0 ? 2 : 4;

                var head = writer.GetSpan(headLen);

                WriteHead(ref head, opCode, totalBytes);
                writer.Advance(headLen);

                var pipelineBuffer = writer.GetSpan(totalBytes).Slice(0, totalBytes);

                bufferSpan.Slice(0, totalBytes).CopyTo(pipelineBuffer);
                writer.Advance(totalBytes);

                return totalBytes + headLen;
            }
            finally
            {
                bufferPool.Return(buffer);
            }
        }

        private int EncodeSingleFragment(IBufferWriter<byte> writer, byte opCode, int expectedHeadLength, ReadOnlySpan<char> text)
        {
            var head = writer.GetSpan(expectedHeadLength);

            writer.Advance(expectedHeadLength);

            var totalBytes = text.Length > 0 ? writer.Write(text, _textEncoding) : 0;

            WriteHead(ref head, (byte)(opCode | 0x80), totalBytes);

            return totalBytes + expectedHeadLength;
        }

        public int EncodeDataMessage(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            var head = writer.GetSpan(10);

            var headLen = WriteHead(ref head, (byte)(pack.OpCodeByte | 0x80), pack.Data.Length);
            
            writer.Advance(headLen);

            foreach (var dataPiece in pack.Data)
            {
                writer.Write(dataPiece.Span);
            }

            return (int)(pack.Data.Length + headLen);
        }

        private (int headLen, int fragmentSize, bool bufferWrite) GetEstimateFragmentation(int msgSize)
        {
            var minSize = msgSize;
            var maxSize = _textEncoding.GetMaxByteCount(msgSize);

            var fragmentSize = 0;
            var headLen = 0;
            var bufferWrite = false;

            if (maxSize < _size0)
                headLen =  2;
            else if (minSize >= _size0 && maxSize < _size1)
                headLen = 4;
            else if (minSize >= _size1)
            {
                headLen = 4;
                fragmentSize = _size1 - 1;
            }

            if (headLen == 0)
            {
                if (minSize < _size0 && maxSize >= _size0)
                {
                    headLen =  2;
                    fragmentSize = _size0 - 1;
                }
                else
                {
                    headLen =  4;
                    fragmentSize = _size1 - 1;
                    bufferWrite = true;
                }
            }

            return (headLen, fragmentSize, bufferWrite);
        }
        
        public int Encode(IBufferWriter<byte> writer, WebSocketPackage pack)
        {
            pack.SaveOpCodeByte();

            var extensions = Extensions;

            if (extensions != null && extensions.Count > 0)
            {
                foreach (var ext in extensions)
                {
                    ext.Encode(pack);
                }
            }

            if (!pack.Data.IsEmpty)
                return EncodeDataMessage(writer, pack);

            var msgSize = !string.IsNullOrEmpty(pack.Message) ? pack.Message.Length : 0;

            if (msgSize == 0)
                return EncodeEmptyFragment(writer, pack.OpCodeByte, 2);

            var total = 0;
            var text = pack.Message.AsSpan();

            var encoder = _textEncoding.GetEncoder();

            var isContinuation = false;

            while (true)
            {
                (var headLen, var fragmentSize, var bufferWrite) = GetEstimateFragmentation(text.Length);

                if (fragmentSize == 0)
                {
                    total += EncodeSingleFragment(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, headLen, text);
                    break;
                }

                var charsUsed = 0;

                if (!bufferWrite)
                    total += EncodeFragment(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, headLen, fragmentSize, text, encoder, out charsUsed);
                else
                    total += EncodeFragmentWithBuffer(writer, isContinuation ? (byte)OpCode.Continuation : pack.OpCodeByte, fragmentSize, text, encoder, out charsUsed);

                if (text.Length <= charsUsed)
                    break;

                text = text.Slice(charsUsed);
                
                if (!isContinuation)
                    isContinuation = true;
            }
            
            return total;
        }
    }
}