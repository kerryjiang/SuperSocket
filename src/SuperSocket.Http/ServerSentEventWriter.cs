using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Http
{
    /// <summary>
    /// Provides functionality for writing Server-Sent Events (SSE) to an HTTP connection.
    /// </summary>
    public class ServerSentEventWriter
    {
        private readonly IConnection _connection;
        private readonly ServerSentEventsOptions _options;
        private long _eventId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSentEventWriter"/> class.
        /// </summary>
        /// <param name="connection">The connection to write SSE data to.</param>
        /// <param name="options">Optional configuration options for SSE behavior.</param>
        public ServerSentEventWriter(IConnection connection, ServerSentEventsOptions options = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options ?? new ServerSentEventsOptions();
        }

        /// <summary>
        /// Sends the initial SSE response headers.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async ValueTask SendInitialResponseAsync(CancellationToken cancellationToken = default)
        {
            var response = new HttpResponse();
            response.SetupForServerSentEvents();
            
            // Apply CORS settings from options
            if (_options.EnableCors)
            {
                response.Headers["Access-Control-Allow-Origin"] = _options.CorsOrigin;
                response.Headers["Access-Control-Allow-Headers"] = _options.CorsAllowedHeaders;
            }
            
            await _connection.SendAsync(HttpResponseEncoder.Instance, response, cancellationToken);
        }

        /// <summary>
        /// Sends a Server-Sent Event with the specified data.
        /// </summary>
        /// <param name="data">The event data to send.</param>
        /// <param name="eventType">The type of the event (optional).</param>
        /// <param name="eventId">The ID of the event (optional, auto-generated if not provided).</param>
        /// <param name="retry">The retry interval in milliseconds (optional, uses default from options if not provided).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async ValueTask SendEventAsync(
            string data, 
            string eventType = null, 
            string eventId = null, 
            int? retry = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Event data cannot be null or empty.", nameof(data));

            var eventBuilder = new StringBuilder();

            // Event ID
            if (!string.IsNullOrEmpty(eventId))
            {
                eventBuilder.AppendLine($"id: {eventId}");
            }
            else
            {
                var autoId = Interlocked.Increment(ref _eventId);
                eventBuilder.AppendLine($"id: {autoId}");
            }

            // Event type
            if (!string.IsNullOrEmpty(eventType))
            {
                eventBuilder.AppendLine($"event: {eventType}");
            }

            // Retry interval
            var retryValue = retry ?? _options.DefaultRetryIntervalMs;
            if (retryValue > 0)
            {
                eventBuilder.AppendLine($"retry: {retryValue}");
            }

            // Data (can be multi-line)
            var dataLines = data.Split('\n');
            foreach (var line in dataLines)
            {
                eventBuilder.AppendLine($"data: {line}");
            }

            // End with empty line
            eventBuilder.AppendLine();

            var eventBytes = Encoding.UTF8.GetBytes(eventBuilder.ToString());
            await _connection.SendAsync(eventBytes, cancellationToken);
        }

        /// <summary>
        /// Sends a heartbeat event to keep the connection alive.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async ValueTask SendHeartbeatAsync(CancellationToken cancellationToken = default)
        {
            // Send a comment line as heartbeat
            var heartbeat = ": heartbeat\n\n";
            var heartbeatBytes = Encoding.UTF8.GetBytes(heartbeat);
            await _connection.SendAsync(heartbeatBytes, cancellationToken);
        }

        /// <summary>
        /// Sends a JSON event with the specified data.
        /// </summary>
        /// <param name="jsonData">The JSON data to send.</param>
        /// <param name="eventType">The type of the event (optional).</param>
        /// <param name="eventId">The ID of the event (optional, auto-generated if not provided).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async ValueTask SendJsonEventAsync(
            string jsonData, 
            string eventType = "json", 
            string eventId = null, 
            CancellationToken cancellationToken = default)
        {
            await SendEventAsync(jsonData, eventType, eventId, null, cancellationToken);
        }

        /// <summary>
        /// Sends a close event to signal the end of the event stream.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async ValueTask SendCloseEventAsync(CancellationToken cancellationToken = default)
        {
            await SendEventAsync("close", "close", null, null, cancellationToken);
        }

        /// <summary>
        /// Starts a background heartbeat task that sends periodic heartbeat events.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the heartbeat operation.</returns>
        public async Task StartHeartbeatAsync(CancellationToken cancellationToken = default)
        {
            if (_options.HeartbeatIntervalSeconds <= 0)
                return;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.HeartbeatIntervalSeconds), cancellationToken);
                    await SendHeartbeatAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception)
                {
                    // Connection may be closed, stop heartbeat
                    break;
                }
            }
        }
    }
}