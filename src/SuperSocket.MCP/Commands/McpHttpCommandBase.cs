using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Command;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP.Commands
{
    /// <summary>
    /// Base class for MCP commands that handle HTTP requests containing MCP messages
    /// </summary>
    public abstract class McpHttpCommandBase : IAsyncCommand<McpHttpRequest>
    {
        protected readonly ILogger _logger;
        protected readonly IMcpHandlerRegistry _handlerRegistry;

        /// <summary>
        /// Initializes a new instance of the McpHttpCommandBase class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry for accessing registered handlers</param>
        protected McpHttpCommandBase(ILogger logger, IMcpHandlerRegistry handlerRegistry)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
        }

        /// <inheritdoc />
        public async ValueTask ExecuteAsync(IAppSession session, McpHttpRequest package, CancellationToken cancellationToken)
        {
            try
            {
                // Extract MCP message from HTTP request
                if (package.McpMessage == null)
                {
                    await session.SendHttpMcpErrorAsync(null, McpErrorCodes.InvalidRequest, "No MCP message found in HTTP request", cancellationToken: cancellationToken);
                    return;
                }

                var response = await HandleAsync(session, package.McpMessage, cancellationToken);
                if (response != null)
                {
                    await session.SendHttpMcpResponseAsync(response, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing MCP HTTP command");
                var errorResponse = CreateErrorResponse(package.McpMessage?.Id, McpErrorCodes.InternalError, "Internal server error");
                await session.SendHttpMcpResponseAsync(errorResponse, cancellationToken);
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