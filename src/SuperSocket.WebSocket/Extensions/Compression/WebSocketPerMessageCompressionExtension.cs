using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
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

        private static readonly byte[] LAST_FOUR_OCTETS = new byte[] { 0x00, 0x00, 0xFF, 0xFF };
        private static readonly byte[] LAST_FOUR_OCTETS_REVERSE = new byte[] { 0xFF, 0xFF, 0x00, 0x00 };

        private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

        public void Decode(IWebSocketFrameHeader websocketFrameHeader, ref ReadOnlySequence<byte> data)
        {
            if (!websocketFrameHeader.RSV1)
                return;

            data = data.ConcactSequence(new SequenceSegment(LAST_FOUR_OCTETS_REVERSE, LAST_FOUR_OCTETS_REVERSE.Length, false));

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

            data = new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        public void Encode(ref ReadOnlySequence<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
