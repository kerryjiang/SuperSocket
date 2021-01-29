using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Extensions.Compression
{
    /// <summary>
    /// WebSocket Per-Message Compression Extension
    /// https://tools.ietf.org/html/rfc7692
    /// </summary>
    public class WebSocketPerMessageCompressionExtension : IWebSocketExtension
    {
        public string Name => PMCE;

        public const string PMCE = "permessage-deflate";

        private const int _deflateBufferSize = 1024 * 1024 * 4;

        private static readonly Encoding _encoding = new UTF8Encoding(false);

        private static readonly byte[] LAST_FOUR_OCTETS = new byte[] { 0x00, 0x00, 0xFF, 0xFF };
        private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

        public void Decode(WebSocketPackage package)
        {
            if (!package.RSV1)
                return;

            var data = package.Data;

            data = data.ConcatSequence(new SequenceSegment(LAST_FOUR_OCTETS, LAST_FOUR_OCTETS.Length, false));

            SequenceSegment head = null;
            SequenceSegment tail = null;

            using (var stream = new DeflateStream(new ReadOnlySequenceStream(data), CompressionMode.Decompress))
            {
                while (true)
                {
                    var buffer = _arrayPool.Rent(_deflateBufferSize);
                    var read = stream.Read(buffer, 0, buffer.Length);

                    if (read == 0)
                        break;

                    var segment = new SequenceSegment(buffer, read);

                    if (head == null)
                        tail = head = segment;
                    else
                        tail.SetNext(segment);
                }
            }

            package.Data = new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        public void Encode(WebSocketPackage package)
        {
            package.RSV1 = true;

            if (package.Data.IsEmpty)
                EncodeTextMessage(package);
            else
                EncodeDataMessage(package);            
        }

        private void EncodeTextMessage(WebSocketPackage package)
        {
            var encoder = _encoding.GetEncoder();
            var text = package.Message.AsSpan();

            var completed = false;      

            var outputStream = new WritableSequenceStream();

            using (var stream = new DeflateStream(outputStream, CompressionMode.Compress))
            {
                while (!completed)
                {
                    var buffer = _arrayPool.Rent(_deflateBufferSize);
                    Span<byte> span = buffer;

                    encoder.Convert(text, span, false, out int charsUsed, out int bytesUsed, out completed);
                
                    if (charsUsed > 0)
                        text = text.Slice(charsUsed);

                    stream.Write(buffer, 0, bytesUsed);
                }

                stream.Flush();
            }

            var data = outputStream.GetUnderlyingSequence();
            RemoveLastFourOctets(ref data);
            package.Data = data;
        }

        private void RemoveLastFourOctets(ref ReadOnlySequence<byte> data)
        {
            var octetsLen = LAST_FOUR_OCTETS.Length;

            if (data.Length < octetsLen)
                return;

            var lastFourBytes = data.Slice(data.Length - octetsLen, octetsLen);
            var pos = 0;

            foreach (var piece in lastFourBytes)
            {
                for (var i = 0; i < piece.Length; i++)
                {
                    if (piece.Span[i] != LAST_FOUR_OCTETS[pos++])
                        return;
                }
            }

            data = data.Slice(0, data.Length - octetsLen);
        }

        private void EncodeDataMessage(WebSocketPackage package)
        {
            var data = package.Data;

            var outputStream = new WritableSequenceStream();

            using (var stream = new DeflateStream(outputStream, CompressionMode.Compress))
            {
                foreach (var piece in data)
                {
                    stream.Write(piece.Span);
                }

                stream.Flush();
            }

            data = outputStream.GetUnderlyingSequence();
            RemoveLastFourOctets(ref data);
            package.Data = data;
        }
    }
}
