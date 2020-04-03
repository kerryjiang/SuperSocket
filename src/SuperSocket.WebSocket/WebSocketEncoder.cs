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
                    head[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }

                return 10;
            }
        }

        private int EncodeFragment(IBufferWriter<byte> writer, OpCode opCode, int expectedHeadLength, ReadOnlySpan<char> text, bool isFinal)
        {
            var head = writer.GetSpan(expectedHeadLength);

            var opCodeFlag = (byte)opCode;

            if (isFinal)
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
                return EncodeFragment(writer, pack.OpCode, 2, string.Empty, true);

            var minSzie = msgSize;
            var maxSize = _textEncoding.GetMaxByteCount(msgSize);

            var fragmentSize = 0;
            var headLen = 0;

            if (maxSize < _size0)
                headLen =  2;
            else if (minSzie >= _size0 && maxSize < _size1)
                headLen = 4;
            else if (minSzie >= _size1)
                headLen = 10;

            if (headLen == 0)
            {
                if (minSzie < _size0 || maxSize >= _size0)
                {
                    headLen =  2;
                    fragmentSize = _size0 / 2;
                }
                else
                {
                    headLen =  4;
                    fragmentSize = _size1 / 2;
                }
            }

            var total = 0;
            var text = pack.Message.AsSpan();

            if (fragmentSize == 0)
            {
                total += EncodeFragment(writer, pack.OpCode, headLen, text, true);
            }
            else
            {
                var isFinal = false;
                var isContinuation = false;

                while (!isFinal)
                {
                    isFinal = text.Length <= fragmentSize;
                    var textInFragment = text;
                    
                    if (!isFinal)
                    {
                        textInFragment = text.Slice(0, fragmentSize);
                        text = text.Slice(fragmentSize);
                    }

                    total += EncodeFragment(writer, isContinuation ? OpCode.Continuation : pack.OpCode, headLen, textInFragment, isFinal);
                    
                    if (!isContinuation)
                        isContinuation = true;
                }
            }

            return total;
        }
    }
}