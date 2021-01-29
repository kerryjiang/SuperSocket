using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        private static ReadOnlySpan<byte> _CRLF => new byte[] { (byte)'\r', (byte)'\n' };
        
        private static readonly char _TAB = '\t';

        private static readonly char _COLON = ':';

        private static readonly ReadOnlyMemory<byte> _headerTerminator = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        
        public IPackageDecoder<WebSocketPackage> Decoder { get; set; }

        public IPipelineFilter<WebSocketPackage> NextFilter { get; internal set; }

        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            var terminatorSpan = _headerTerminator.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
                return null;

            reader.Advance(terminatorSpan.Length);

            var package = ParseHandshake(ref pack);

            NextFilter = new WebSocketDataPipelineFilter(package.HttpHeader);
            
            return package;
        }

        private WebSocketPackage ParseHandshake(ref ReadOnlySequence<byte> pack)
        {
            var header = ParseHttpHeaderItems(ref pack);

            return new WebSocketPackage
            {
                HttpHeader = header,
                OpCode = OpCode.Handshake
            };
        }

        private bool TryParseHttpHeaderItems(ref ReadOnlySequence<byte> header, out string firstLine, out NameValueCollection items)
        {
            var headerText = header.GetString(Encoding.UTF8);
            var reader = new StringReader(headerText);
            firstLine = reader.ReadLine();

            if (string.IsNullOrEmpty(firstLine))
            {
                items = null;
                return false;
            }

            items = new NameValueCollection();

            var prevKey = string.Empty;
            var line = string.Empty;
            
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith(_TAB) && !string.IsNullOrEmpty(prevKey))
                {
                    var currentValue = items.Get(prevKey);
                    items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(_COLON);

                if (pos <= 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                if (string.IsNullOrEmpty(key))
                    continue;

                var valueOffset = pos + 1;

                if (line.Length <= valueOffset) //No value in this line
                    continue;

                var value = line.Substring(valueOffset);

                if (!string.IsNullOrEmpty(value) && value.StartsWith(' ') && value.Length > 1)
                    value = value.Substring(1);

                var existingValue = items.Get(key);

                if (string.IsNullOrEmpty(existingValue))
                {
                    items.Add(key, value);
                }
                else
                {
                    items[key] = existingValue + ", " + value;
                }

                prevKey = key;
            }

            return true;
        }

        protected virtual HttpHeader CreateHttpHeader(string verbItem1, string verbItem2, string verbItem3, NameValueCollection items)
        {
            return HttpHeader.CreateForRequest(verbItem1, verbItem2, verbItem3, items);
        }

        private HttpHeader ParseHttpHeaderItems(ref ReadOnlySequence<byte> header)
        {
            if (!TryParseHttpHeaderItems(ref header, out var firstLine, out var items))
                return null;

            var verbItems = firstLine.Split(' ', 3);

            if (verbItems.Length < 3)
            {
                // invalid first line
                return null;
            }

            return CreateHttpHeader(verbItems[0], verbItems[1], verbItems[2], items);
        }

        public void Reset()
        {
            
        }

        public object Context { get; set; }
    }
}
