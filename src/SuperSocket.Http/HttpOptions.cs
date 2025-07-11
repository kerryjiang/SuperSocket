using System;

namespace SuperSocket.Http
{
    /// <summary>
    /// Configuration options for HTTP Keep-Alive functionality.
    /// </summary>
    public class HttpKeepAliveOptions
    {
        /// <summary>
        /// Gets or sets the timeout for keep-alive connections in seconds.
        /// Default is 30 seconds.
        /// </summary>
        public int KeepAliveTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of requests allowed per connection.
        /// Default is 1000. Set to 0 for unlimited.
        /// </summary>
        public int MaxRequestsPerConnection { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether keep-alive is enabled by default.
        /// Default is true for HTTP/1.1.
        /// </summary>
        public bool EnableKeepAlive { get; set; } = true;
    }

    /// <summary>
    /// Configuration options for Server-Sent Events functionality.
    /// </summary>
    public class ServerSentEventsOptions
    {
        /// <summary>
        /// Gets or sets the heartbeat interval in seconds.
        /// Default is 30 seconds. Set to 0 to disable heartbeat.
        /// </summary>
        public int HeartbeatIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the retry interval suggested to clients in milliseconds.
        /// Default is 3000 (3 seconds).
        /// </summary>
        public int DefaultRetryIntervalMs { get; set; } = 3000;

        /// <summary>
        /// Gets or sets a value indicating whether CORS headers should be automatically added.
        /// Default is true.
        /// </summary>
        public bool EnableCors { get; set; } = true;

        /// <summary>
        /// Gets or sets the CORS origin header value.
        /// Default is "*" (allow all origins).
        /// </summary>
        public string CorsOrigin { get; set; } = "*";

        /// <summary>
        /// Gets or sets the allowed CORS headers.
        /// Default is "Cache-Control".
        /// </summary>
        public string CorsAllowedHeaders { get; set; } = "Cache-Control";
    }
}