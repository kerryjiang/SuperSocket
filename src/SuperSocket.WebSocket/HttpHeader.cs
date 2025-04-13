using System;
using System.Buffers;
using System.Collections.Specialized;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Represents an HTTP header for WebSocket requests and responses.
    /// </summary>
    public class HttpHeader
    {
        /// <summary>
        /// Gets the HTTP method of the request.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Gets the path of the HTTP request.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the HTTP version of the request or response.
        /// </summary>
        public string HttpVersion { get; private set; }

        /// <summary>
        /// Gets the status code of the HTTP response.
        /// </summary>
        public string StatusCode { get; private set; }

        /// <summary>
        /// Gets the status description of the HTTP response.
        /// </summary>
        public string StatusDescription { get; private set; }

        /// <summary>
        /// Gets the collection of HTTP header items.
        /// </summary>
        public NameValueCollection Items { get; private set; }

        private HttpHeader()
        {
            
        }

        /// <summary>
        /// Creates an HTTP header for a request.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="httpVersion">The HTTP version.</param>
        /// <param name="items">The collection of HTTP header items.</param>
        /// <returns>A new instance of the <see cref="HttpHeader"/> class.</returns>
        public static HttpHeader CreateForRequest(string method, string path, string httpVersion, NameValueCollection items)
        {
            return new HttpHeader
            {
                Method = method,
                Path = path,
                HttpVersion = httpVersion,
                Items = items
            };
        }

        /// <summary>
        /// Creates an HTTP header for a response.
        /// </summary>
        /// <param name="httpVersion">The HTTP version.</param>
        /// <param name="statusCode">The status code of the response.</param>
        /// <param name="statusDescription">The status description of the response.</param>
        /// <param name="items">The collection of HTTP header items.</param>
        /// <returns>A new instance of the <see cref="HttpHeader"/> class.</returns>
        public static HttpHeader CreateForResponse(string httpVersion, string statusCode, string statusDescription, NameValueCollection items)
        {
            return new HttpHeader
            {
                HttpVersion = httpVersion,
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                Items = items
            };
        }
    }
}
