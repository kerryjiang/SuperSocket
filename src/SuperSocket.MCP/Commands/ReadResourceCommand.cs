using System;
using System.Collections.Generic;
using System.Text.Json;
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
    /// Command to handle MCP resources/read requests
    /// </summary>
    [Command("resources/read")]
    public class ReadResourceCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the ReadResourceCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public ReadResourceCommand(ILogger<ReadResourceCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var readParams = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(message.Params),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (readParams == null || !readParams.TryGetValue("uri", out var uriObj))
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "URI parameter required");
                }

                var uri = uriObj?.ToString();
                if (string.IsNullOrEmpty(uri))
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "URI cannot be empty");
                }

                var handler = _handlerRegistry.GetResourceHandler(uri);
                if (handler == null)
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Resource '{uri}' not found");
                }

                _logger.LogInformation("Reading resource: {ResourceUri}", uri);

                var content = await handler.ReadAsync(uri);

                var result = new { contents = new[] { content } };

                _logger.LogInformation("Resource {ResourceUri} read successfully", uri);

                return CreateSuccessResponse(message.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading resource");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Resource read failed");
            }
        }
    }
}