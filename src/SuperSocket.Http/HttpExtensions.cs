using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Http
{
    /// <summary>
    /// Extension methods for easier HTTP functionality usage.
    /// </summary>
    public static class HttpExtensions
    {
        /// <summary>
        /// Sends an HTTP response to the session.
        /// </summary>
        /// <param name="session">The session to send the response to.</param>
        /// <param name="response">The HTTP response to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async ValueTask SendHttpResponseAsync(
            this IAppSession session, 
            HttpResponse response, 
            CancellationToken cancellationToken = default)
        {
            await session.SendAsync(HttpResponseEncoder.Instance, response, cancellationToken);
        }

        /// <summary>
        /// Sends a simple HTTP response with the specified status and body.
        /// </summary>
        /// <param name="session">The session to send the response to.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="body">The response body.</param>
        /// <param name="contentType">The content type. Defaults to "text/plain".</param>
        /// <param name="keepAlive">Whether to keep the connection alive.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async ValueTask SendHttpResponseAsync(
            this IAppSession session,
            int statusCode,
            string body = "",
            string contentType = "text/plain",
            bool keepAlive = true,
            CancellationToken cancellationToken = default)
        {
            var response = new HttpResponse(statusCode);
            response.SetContentType(contentType);
            response.Body = body;
            response.KeepAlive = keepAlive;

            await session.SendHttpResponseAsync(response, cancellationToken);
        }

        /// <summary>
        /// Sends a JSON HTTP response.
        /// </summary>
        /// <param name="session">The session to send the response to.</param>
        /// <param name="jsonData">The JSON data to send.</param>
        /// <param name="statusCode">The HTTP status code. Defaults to 200.</param>
        /// <param name="keepAlive">Whether to keep the connection alive.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async ValueTask SendJsonResponseAsync(
            this IAppSession session,
            string jsonData,
            int statusCode = 200,
            bool keepAlive = true,
            CancellationToken cancellationToken = default)
        {
            await session.SendHttpResponseAsync(statusCode, jsonData, "application/json", keepAlive, cancellationToken);
        }

        /// <summary>
        /// Creates a Server-Sent Events writer for the session.
        /// </summary>
        /// <param name="session">The session to create the SSE writer for.</param>
        /// <param name="options">Optional SSE configuration options.</param>
        /// <returns>A new ServerSentEventWriter instance.</returns>
        public static ServerSentEventWriter CreateSSEWriter(this IAppSession session, ServerSentEventsOptions options = null)
        {
            return new ServerSentEventWriter(session.Connection, options);
        }

        /// <summary>
        /// Starts a Server-Sent Events stream for the session.
        /// </summary>
        /// <param name="session">The session to start SSE for.</param>
        /// <param name="options">Optional SSE configuration options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A new ServerSentEventWriter instance that has already sent the initial response.</returns>
        public static async ValueTask<ServerSentEventWriter> StartSSEAsync(
            this IAppSession session, 
            ServerSentEventsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var writer = session.CreateSSEWriter(options);
            await writer.SendInitialResponseAsync(cancellationToken);
            return writer;
        }

        /// <summary>
        /// Checks if the HTTP request accepts Server-Sent Events.
        /// </summary>
        /// <param name="request">The HTTP request to check.</param>
        /// <returns>True if the request accepts SSE, false otherwise.</returns>
        public static bool IsSSERequest(this HttpRequest request)
        {
            return request.AcceptsEventStream;
        }

        /// <summary>
        /// Checks if the HTTP request wants to keep the connection alive.
        /// </summary>
        /// <param name="request">The HTTP request to check.</param>
        /// <returns>True if keep-alive is requested, false otherwise.</returns>
        public static bool IsKeepAliveRequest(this HttpRequest request)
        {
            return request.KeepAlive;
        }
    }
}