using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketEncoder : IPackageEncoder<WebSocketMessage>
    {
        private static readonly Encoding _textEncoding = Encoding.UTF8;

        private const int _size0 = 126;
        private const int _size1 = 65536;

        private int WriteLength(ref Span<byte> head, long length)
        {
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

        private int EncodeEmptyFragment(IBufferWriter<byte> writer, OpCode opCode, int expectedHeadLength)
        {
            return EncodeSingleFragment(writer, opCode, expectedHeadLength, default);
        }

        private int EncodeFragment(IBufferWriter<byte> writer, OpCode opCode, int expectedHeadLength, int fragmentSize, ReadOnlySpan<char> text, Encoder encoder, out int charsUsed)
        {
            charsUsed = 0;

            var head = writer.GetSpan(expectedHeadLength);

            var opCodeFlag = (byte)opCode;            

            writer.Advance(expectedHeadLength);

            var buffer = writer.GetSpan(fragmentSize).Slice(0, fragmentSize);
            
            encoder.Convert(text, buffer, false, out charsUsed, out int bytesUsed, out bool completed);
            writer.Advance(bytesUsed);

            var totalBytes = bytesUsed;
            var isFinal = completed && text.Length == charsUsed;

            if (isFinal)
                opCodeFlag = (byte)(opCodeFlag | 0x80);

            head[0] = opCodeFlag;

            WriteLength(ref head, totalBytes);

            return totalBytes + expectedHeadLength;
        }

        private int EncodeSingleFragment(IBufferWriter<byte> writer, OpCode opCode, int expectedHeadLength, ReadOnlySpan<char> text)
        {
            var head = writer.GetSpan(expectedHeadLength);

            var opCodeFlag = (byte)opCode;

            opCodeFlag = (byte)(opCodeFlag | 0x80);

            head[0] = opCodeFlag;

            writer.Advance(expectedHeadLength);

            var totalBytes = text.Length > 0 ? writer.Write(text, _textEncoding) : 0;

            WriteLength(ref head, totalBytes);

            return totalBytes + expectedHeadLength;
        }

        public int EncodeBinaryMessage(IBufferWriter<byte> writer, WebSocketMessage pack)
        {
            var head = writer.GetSpan(10);

            head[0] = (byte)((byte)pack.OpCode | 0x80);

            var headLen = WriteLength(ref head, pack.Data.Length);
            
            writer.Advance(headLen);

            foreach (var dataPiece in pack.Data)
            {
                writer.Write(dataPiece.Span);
            }

            return (int)(pack.Data.Length + headLen);
        }
        
        public int Encode(IBufferWriter<byte> writer, WebSocketMessage pack)
        {
            if (pack.OpCode != OpCode.Text)
                return EncodeBinaryMessage(writer, pack);

            var msgSize = !string.IsNullOrEmpty(pack.Message) ? pack.Message.Length : 0;

            if (msgSize == 0)
                return EncodeEmptyFragment(writer, pack.OpCode, 2);

            var minSize = msgSize;
            var maxSize = _textEncoding.GetMaxByteCount(msgSize);

            var fragmentSize = 0;
            var headLen = 0;

            if (maxSize < _size0)
                headLen =  2;
            else if (minSize >= _size0 && maxSize < _size1)
                headLen = 4;
            else if (minSize >= _size1)
                headLen = 10;

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
                }
            }

            var total = 0;
            var text = pack.Message.AsSpan();

            if (fragmentSize == 0)
            {
                total += EncodeSingleFragment(writer, pack.OpCode, headLen, text);
            }
            else
            {
                var isContinuation = false;
                var encoder = _textEncoding.GetEncoder();

                while (true)
                {
                    total += EncodeFragment(writer, isContinuation ? OpCode.Continuation : pack.OpCode, headLen, fragmentSize, text, encoder, out int charsUsed);

                    if (text.Length <= charsUsed)
                        break;

                    text = text.Slice(charsUsed);
                    
                    if (!isContinuation)
                        isContinuation = true;
                }
            }

            return total;
        }
    }
}