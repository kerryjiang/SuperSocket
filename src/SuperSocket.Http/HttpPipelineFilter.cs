using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.Http
{
    public class HttpPipelineFilter : IPipelineFilter<HttpRequest>
    {
        private static ReadOnlySpan<byte> _CRLF => new byte[] { (byte)'\r', (byte)'\n' };
        
        private static readonly char _TAB = '\t';

        private static readonly char _COLON = ':';

        private static readonly ReadOnlyMemory<byte> _headerTerminator = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        
        public IPackageDecoder<HttpRequest> Decoder { get; set; }

        public IPipelineFilter<HttpRequest> NextFilter { get; internal set; }

        private HttpRequest _currentRequest;

        private long _bodyLength;

        public HttpRequest Filter(ref SequenceReader<byte> reader)
        {
            if (_bodyLength == 0)
            {
                var terminatorSpan = _headerTerminator.Span;

                if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
                    return null;

                reader.Advance(terminatorSpan.Length);
                
                var request = ParseHttpHeaderItems(pack);

                var contentLength = request.Items?["content-length"];

                if (string.IsNullOrEmpty(contentLength)) // no content
                    return request;

                var bodyLength = long.Parse(contentLength);

                if (bodyLength == 0)
                    return request;
                    
                _bodyLength = bodyLength;
                _currentRequest = request;

                return Filter(ref reader);
            }

            if (reader.Remaining < _bodyLength)
                return null;

            var seq = reader.Sequence;

            _currentRequest.Body = seq.Slice(reader.Consumed, _bodyLength).GetString(Encoding.UTF8);
            reader.Advance(_bodyLength);

            return _currentRequest;
        }

        private HttpRequest ParseHttpHeaderItems(ReadOnlySequence<byte> header)
        {
            var headerText = header.GetString(Encoding.UTF8);
            var reader = new StringReader(headerText);
            var firstLine = reader.ReadLine();

            if (string.IsNullOrEmpty(firstLine))
                return null;

            var items = new NameValueCollection();

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

            var metaInfo = firstLine.Split(' ');

            if (metaInfo.Length != 3)
            {
                // invalid first line
                return null;
            }

            return new HttpRequest(metaInfo[0], metaInfo[1], metaInfo[2], items);
        }

        public void Reset()
        {
            _bodyLength = 0;
            _currentRequest = null;
        }

        public object Context { get; set; }
    }
}
