using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Command;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP.Commands
{
    /// <summary>
    /// Base class for MCP commands that handle McpMessage packets
    /// </summary>
    public abstract class McpCommandBase : IAsyncCommand<McpMessage>
    {
        protected readonly ILogger _logger;
        protected readonly IMcpHandlerRegistry _handlerRegistry;

        /// <summary>
        /// Initializes a new instance of the McpCommandBase class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry for accessing registered handlers</param>
        protected McpCommandBase(ILogger logger, IMcpHandlerRegistry handlerRegistry)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
        }

        /// <inheritdoc />
        public async ValueTask ExecuteAsync(IAppSession session, McpMessage package, CancellationToken cancellationToken)
        {
            try
            {
                // Handle notifications separately - they don't need responses
                if (package.IsNotification)
                {
                    await McpNotificationHandler.HandleNotificationAsync(session, package, _logger, cancellationToken);
                    return;
                }

                var response = await HandleAsync(session, package, cancellationToken);
                if (response != null)
                {
                    await SendResponseAsync(session, response, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing MCP command");
                var errorResponse = CreateErrorResponse(package.Id, McpErrorCodes.InternalError, "Internal server error");
                await SendResponseAsync(session, errorResponse, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the MCP command logic and returns the response message
        /// </summary>
        /// <param name="session">Application session</param>
        /// <param name="message">Incoming MCP message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message, or null if no response should be sent</returns>
        protected abstract Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a response message back to the client
        /// </summary>
        /// <param name="session">Application session</param>
        /// <param name="response">Response message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        protected virtual async Task SendResponseAsync(IAppSession session, McpMessage response, CancellationToken cancellationToken)
        {
            // Default implementation - serialize to JSON and send as bytes
            // This will be overridden by transport-specific implementations
            var json = System.Text.Json.JsonSerializer.Serialize(response);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await session.SendAsync(bytes, cancellationToken);
        }

        /// <summary>
        /// Creates a success response message
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="result">Result object</param>
        /// <returns>Success response message</returns>
        protected static McpMessage CreateSuccessResponse(object? id, object result)
        {
            return new McpMessage
            {
                Id = id,
                Result = result
            };
        }

        /// <summary>
        /// Creates an error response message
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        /// <returns>Error response message</returns>
        protected static McpMessage CreateErrorResponse(object? id, int code, string message)
        {
            return new McpMessage
            {
                Id = id,
                Error = new McpError
                {
                    Code = code,
                    Message = message
                }
            };
        }
    }
}