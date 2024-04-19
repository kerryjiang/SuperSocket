using System;
using System.Buffers;
using System.Collections.Specialized;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public sealed class WebSocketPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        private static ReadOnlySpan<byte> NewLine => new byte[] { (byte)'\r', (byte)'\n' };
        private static ReadOnlySpan<byte> TrimChars => new byte[] { (byte)' ', (byte)'\t' };

        public IPackageDecoder<WebSocketPackage> Decoder { get; set; }

        public IPipelineFilter<WebSocketPackage> NextFilter { get; internal set; }

        public object Context { get; set; }

        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            if (!reader.TryReadTo(out ReadOnlySpan<byte> methodSpan, (byte)' '))
                return null;

            if (!reader.TryReadTo(out ReadOnlySpan<byte> pathSpan, (byte)' '))
                return null;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> versionSpan, NewLine))
                return null;

            var method = Encoding.ASCII.GetString(methodSpan);
            var requestUri = Encoding.ASCII.GetString(pathSpan);
            var version = versionSpan.GetString(Encoding.ASCII);

            var items = new NameValueCollection();

            while (reader.TryReadTo(out ReadOnlySequence<byte> headerLine, NewLine))
            {
                if (headerLine.Length == 0)
                    break;

                ParseHeader(headerLine, out var headerName, out var headerValue);

#if NET5_0_OR_GREATER
                var key = Encoding.ASCII.GetString(headerName.Trim(TrimChars));
                var value = Encoding.ASCII.GetString(headerValue.Trim(TrimChars));
#else
                var key = Encoding.ASCII.GetString(headerName);
                var value = Encoding.ASCII.GetString(headerValue);
#endif
                items.Add(key, value);
            }

            var header = HttpHeader.CreateForRequest(method, requestUri, version, items);

            NextFilter = new WebSocketDataPipelineFilter(header);

            return new WebSocketPackage
            {
                HttpHeader = header,
                OpCode = OpCode.Handshake
            };
        }

        private void ParseHeader(in ReadOnlySequence<byte> headerLine, out ReadOnlySpan<byte> headerName,
            out ReadOnlySpan<byte> headerValue)
        {
            if (headerLine.IsSingleSegment)
            {
                var span = headerLine.FirstSpan;
                var colon = span.IndexOf((byte)':');
                headerName = span.Slice(0, colon);
                headerValue = span.Slice(colon + 1);
            }
            else
            {
                var headerReader = new SequenceReader<byte>(headerLine);
                headerReader.TryReadTo(out headerName, (byte)':');
                var remaining = headerReader.Sequence.Slice(headerReader.Position);
                headerValue = remaining.IsSingleSegment ? remaining.FirstSpan : remaining.ToArray();
            }
        }

        public void Reset()
        {
        }
    }
}