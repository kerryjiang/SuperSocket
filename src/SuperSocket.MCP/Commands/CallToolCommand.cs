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
    /// Command to handle MCP tools/call requests
    /// </summary>
    [Command("tools/call")]
    public class CallToolCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the CallToolCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public CallToolCommand(ILogger<CallToolCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var callParams = JsonSerializer.Deserialize<McpCallToolParams>(
                    JsonSerializer.Serialize(message.Params),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (callParams == null || string.IsNullOrEmpty(callParams.Name))
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "Invalid tool call parameters");
                }

                var handler = _handlerRegistry.GetToolHandler(callParams.Name);
                if (handler == null)
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Tool '{callParams.Name}' not found");
                }

                _logger.LogInformation("Executing tool: {ToolName}", callParams.Name);

                var toolResult = await handler.ExecuteAsync(callParams.Arguments ?? new Dictionary<string, object>());

                var result = new McpCallToolResult
                {
                    Content = toolResult.Content,
                    IsError = toolResult.IsError
                };

                _logger.LogInformation("Tool {ToolName} executed successfully", callParams.Name);

                return CreateSuccessResponse(message.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Tool execution failed");
            }
        }
    }
}