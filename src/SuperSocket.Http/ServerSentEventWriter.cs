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
        private long _eventId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSentEventWriter"/> class.
        /// </summary>
        /// <param name="connection">The connection to write SSE data to.</param>
        public ServerSentEventWriter(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
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
            
            var responseBytes = response.ToBytes();
            await _connection.SendAsync(responseBytes, cancellationToken);
        }

        /// <summary>
        /// Sends a Server-Sent Event with the specified data.
        /// </summary>
        /// <param name="data">The event data to send.</param>
        /// <param name="eventType">The type of the event (optional).</param>
        /// <param name="eventId">The ID of the event (optional, auto-generated if not provided).</param>
        /// <param name="retry">The retry interval in milliseconds (optional).</param>
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
            if (retry.HasValue)
            {
                eventBuilder.AppendLine($"retry: {retry.Value}");
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
    }
}