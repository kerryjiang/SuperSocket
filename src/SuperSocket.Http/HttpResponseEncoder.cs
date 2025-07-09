using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.Http
{
    /// <summary>
    /// Encodes HttpResponse objects into byte sequences for transmission over HTTP.
    /// </summary>
    public class HttpResponseEncoder : IPackageEncoder<HttpResponse>
    {
        private readonly Encoding _headerEncoding = Encoding.ASCII;

        private readonly Encoding _bodyEncoding = new UTF8Encoding(false);

        public static readonly HttpResponseEncoder Instance = new HttpResponseEncoder();

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseEncoder"/> class with UTF-8 encoding.
        /// </summary>
        internal HttpResponseEncoder()
        {
        }

        /// <summary>
        /// Encodes an HttpResponse into the specified buffer writer.
        /// </summary>
        /// <param name="writer">The buffer writer to write the encoded response to.</param>
        /// <param name="response">The HttpResponse to encode.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        public int Encode(IBufferWriter<byte> writer, HttpResponse response)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var totalBytes = 0;

            // Write status line
            var statusLine = $"{response.HttpVersion} {response.StatusCode} {response.StatusMessage}\r\n";
            totalBytes += WriteString(writer, statusLine, _headerEncoding);

            // Set Content-Length header if body exists
            if (!string.IsNullOrEmpty(response.Body))
            {
                var bodyBytes = _bodyEncoding.GetByteCount(response.Body);
                response.Headers["Content-Length"] = bodyBytes.ToString();
            }

            // Set Connection header based on KeepAlive
            response.Headers["Connection"] = response.KeepAlive ? "keep-alive" : "close";

            // Add default headers if not present
            if (string.IsNullOrEmpty(response.Headers["Date"]))
                response.Headers["Date"] = DateTime.UtcNow.ToString("r");

            if (string.IsNullOrEmpty(response.Headers["Server"]))
                response.Headers["Server"] = "SuperSocket";

            // Write headers
            foreach (string key in response.Headers.AllKeys)
            {
                var headerLine = $"{key}: {response.Headers[key]}\r\n";
                totalBytes += WriteString(writer, headerLine, _headerEncoding);
            }

            // Write empty line to separate headers from body
            totalBytes += WriteString(writer, "\r\n", _headerEncoding);

            // Write body if present
            if (!string.IsNullOrEmpty(response.Body))
            {
                totalBytes += WriteString(writer, response.Body, _bodyEncoding);
            }

            return totalBytes;
        }

        /// <summary>
        /// Writes a string to the buffer writer using the configured encoding.
        /// </summary>
        /// <param name="writer">The buffer writer to write to.</param>
        /// <param name="text">The text to write.</param>
        /// <param name="encoding">The encoding to use for the text.</param>
        /// <returns>The number of bytes written.</returns>
        private int WriteString(IBufferWriter<byte> writer, string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var byteCount = encoding.GetByteCount(text);
            var span = writer.GetSpan(byteCount);
            var actualBytes = encoding.GetBytes(text, span);
            writer.Advance(actualBytes);
            return actualBytes;
        }
    }
}
