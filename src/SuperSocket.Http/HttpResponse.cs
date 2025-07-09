using System;
using System.Collections.Specialized;
using System.Text;

namespace SuperSocket.Http
{
    /// <summary>
    /// Represents an HTTP response, including status code, headers, and body.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// Gets or sets the HTTP status message of the response.
        /// </summary>
        public string StatusMessage { get; set; } = "OK";

        /// <summary>
        /// Gets or sets the HTTP version of the response.
        /// </summary>
        public string HttpVersion { get; set; } = "HTTP/1.1";

        /// <summary>
        /// Gets the collection of headers associated with the response.
        /// </summary>
        public NameValueCollection Headers { get; private set; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the body of the response.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the connection should be kept alive.
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class.
        /// </summary>
        public HttpResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponse"/> class with the specified status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="statusMessage">The HTTP status message.</param>
        public HttpResponse(int statusCode, string statusMessage = null)
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage ?? GetDefaultStatusMessage(statusCode);
        }
        
        /// <summary>
        /// Sets the content type header.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        public void SetContentType(string contentType)
        {
            Headers["Content-Type"] = contentType;
        }

        /// <summary>
        /// Sets up the response for Server-Sent Events.
        /// </summary>
        public void SetupForServerSentEvents()
        {
            SetContentType("text/event-stream");
            Headers["Cache-Control"] = "no-cache";
            Headers["Connection"] = "keep-alive";
            Headers["Access-Control-Allow-Origin"] = "*";
            Headers["Access-Control-Allow-Headers"] = "Cache-Control";
            KeepAlive = true;
        }

        private static string GetDefaultStatusMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                204 => "No Content",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => "Unknown"
            };
        }
    }
}