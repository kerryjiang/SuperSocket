using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents a pipeline filter for processing WebSocket handshake requests and managing the pipeline context.
    /// </summary>
    public class WebSocketPipelineFilter : IPipelineFilter<WebSocketPackage>
    {
        private static ReadOnlySpan<byte> _CRLF => new byte[] { (byte)'\r', (byte)'\n' };
        
        private static readonly char _TAB = '\t';

        private static readonly char _COLON = ':';

        private static readonly ReadOnlyMemory<byte> _headerTerminator = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        private readonly bool _requireMask = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketPipelineFilter"/> class with default settings.
        /// </summary>
        public WebSocketPipelineFilter()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketPipelineFilter"/> class with the specified mask requirement.
        /// </summary>
        /// <param name="requireMask">Indicates whether a mask is required for WebSocket packages.</param>
        public WebSocketPipelineFilter(bool requireMask)
        {
            _requireMask = requireMask;
        }

        /// <summary>
        /// Gets or sets the decoder for WebSocket packages.
        /// </summary>
        public IPackageDecoder<WebSocketPackage> Decoder { get; set; }

        /// <summary>
        /// Gets or sets the next pipeline filter in the chain.
        /// </summary>
        public IPipelineFilter<WebSocketPackage> NextFilter { get; internal set; }

        /// <summary>
        /// Filters the incoming data and returns a WebSocket package.
        /// </summary>
        /// <param name="reader">The sequence reader for the incoming data.</param>
        /// <returns>The filtered WebSocket package, or null if more data is needed.</returns>
        public WebSocketPackage Filter(ref SequenceReader<byte> reader)
        {
            var terminatorSpan = _headerTerminator.Span;

            if (!reader.TryReadTo(out ReadOnlySequence<byte> pack, terminatorSpan, advancePastDelimiter: false))
                return null;

            reader.Advance(terminatorSpan.Length);

            var package = ParseHandshake(ref pack);

            NextFilter = new WebSocketDataPipelineFilter(package.HttpHeader, _requireMask);
            
            return package;
        }

        /// <summary>
        /// Parses the WebSocket handshake request from the given sequence.
        /// </summary>
        /// <param name="pack">The sequence containing the handshake request.</param>
        /// <returns>A <see cref="WebSocketPackage"/> object representing the handshake request.</returns>
        private WebSocketPackage ParseHandshake(ref ReadOnlySequence<byte> pack)
        {
            var header = ParseHttpHeaderItems(ref pack);

            return new WebSocketPackage
            {
                HttpHeader = header,
                OpCode = OpCode.Handshake
            };
        }

        /// <summary>
        /// Attempts to parse HTTP header items from the given sequence.
        /// </summary>
        /// <param name="header">The sequence containing the HTTP header.</param>
        /// <param name="firstLine">The first line of the HTTP header.</param>
        /// <param name="items">The collection of parsed HTTP header items.</param>
        /// <returns>True if the header items were successfully parsed; otherwise, false.</returns>
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

        /// <summary>
        /// Creates an HTTP header object from the given components.
        /// </summary>
        /// <param name="verbItem1">The first item of the HTTP verb.</param>
        /// <param name="verbItem2">The second item of the HTTP verb.</param>
        /// <param name="verbItem3">The third item of the HTTP verb.</param>
        /// <param name="items">The collection of HTTP header items.</param>
        /// <returns>A <see cref="HttpHeader"/> object representing the HTTP header.</returns>
        protected virtual HttpHeader CreateHttpHeader(string verbItem1, string verbItem2, string verbItem3, NameValueCollection items)
        {
            return HttpHeader.CreateForRequest(verbItem1, verbItem2, verbItem3, items);
        }

        /// <summary>
        /// Parses the HTTP header items from the given sequence.
        /// </summary>
        /// <param name="header">The sequence containing the HTTP header.</param>
        /// <returns>A <see cref="HttpHeader"/> object representing the parsed header.</returns>
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

        /// <summary>
        /// Resets the pipeline filter to its initial state.
        /// </summary>
        public void Reset()
        {
            
        }

        /// <summary>
        /// Gets or sets the context associated with the pipeline filter.
        /// </summary>
        public object Context { get; set; }
    }
}
