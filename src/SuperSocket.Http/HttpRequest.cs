using System;
using System.Collections.Specialized;

namespace SuperSocket.Http
{
    /// <summary>
    /// Represents an HTTP request, including method, path, version, headers, and body.
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// Gets the HTTP method of the request (e.g., GET, POST).
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Gets the path of the request.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the HTTP version of the request.
        /// </summary>
        public string HttpVersion { get; private set; }

        /// <summary>
        /// Gets the collection of headers or other items associated with the request.
        /// </summary>
        public NameValueCollection Items { get; private set; }

        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets a value indicating whether the client requests a keep-alive connection.
        /// </summary>
        public bool KeepAlive => GetKeepAliveFromHeaders();

        /// <summary>
        /// Gets a value indicating whether this request supports Server-Sent Events.
        /// </summary>
        public bool AcceptsEventStream => 
            Items?["Accept"]?.Contains("text/event-stream") == true ||
            Items?["Accept"]?.Contains("*/*") == true;

        /// <summary>
        /// Gets the value of the Last-Event-ID header for SSE reconnection.
        /// </summary>
        public string LastEventId => Items?["Last-Event-ID"];

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequest"/> class with the specified method, path, version, and items.
        /// </summary>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="path">The path of the request.</param>
        /// <param name="httpVersion">The HTTP version of the request.</param>
        /// <param name="items">The collection of headers or other items associated with the request.</param>
        public HttpRequest(string method, string path, string httpVersion, NameValueCollection items)
        {
            Method = method;
            Path = path;
            HttpVersion = httpVersion;
            Items = items;
        }

        private bool GetKeepAliveFromHeaders()
        {
            var connection = Items?["Connection"];
            if (string.IsNullOrEmpty(connection))
            {
                // HTTP/1.1 defaults to keep-alive, HTTP/1.0 defaults to close
                return HttpVersion?.Contains("1.1") == true;
            }

            return connection.ToLowerInvariant().Contains("keep-alive");
        }
    }
}
