using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketEncoder : IPackageEncoder<WebSocketMessage>
    {
        private static readonly Encoding _textEncoding = Encoding.UTF8;

        private int WriteLength(ref Span<byte> head, int length)
        {
            if (length < 126)
            {
                head[1] = (byte)length;
                return 2;
            }
            else if (length < 65536)
            {
                head[1] = (byte)126;
                head[2] = (byte)(length / 256);
                head[3] = (byte)(length % 256);
                return 4;
            }
            else
            {
                head[1] = (byte)127;

                int left = length;
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

        public int Encode(PipeWriter writer, WebSocketMessage pack)
        {
            var head = writer.GetSpan(10);
            var headLen = 0;

            head[0] = (byte)((byte)pack.OpCode | 0x80);

            var dataLength = 0;

            if (pack.OpCode == OpCode.Binary)
            {
                dataLength = (int)pack.Data.Length;
                headLen = WriteLength(ref head, dataLength);
                writer.Write(head.Slice(0, headLen));

                foreach (var dataPiece in pack.Data)
                {
                    writer.Write(dataPiece.Span);
                }
            }
            else
            {
                var textBufferSize = _textEncoding.GetMaxByteCount(pack.Message.Length);
                var textBuffer = writer.GetSpan(textBufferSize);

                dataLength = _textEncoding.GetBytes(pack.Message, textBuffer);
                headLen = WriteLength(ref head, dataLength);
                writer.Write(head.Slice(0, headLen));

                writer.Write(textBuffer.Slice(0, dataLength));
            }

            return headLen + dataLength;
        }
    }
}