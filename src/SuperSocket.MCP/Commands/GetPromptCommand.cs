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
    /// Command to handle MCP prompts/get requests
    /// </summary>
    [Command("prompts/get")]
    public class GetPromptCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the GetPromptCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public GetPromptCommand(ILogger<GetPromptCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var promptParams = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(message.Params),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (promptParams == null || !promptParams.TryGetValue("name", out var nameObj))
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "Prompt name required");
                }

                var name = nameObj?.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "Prompt name cannot be empty");
                }

                var handler = _handlerRegistry.GetPromptHandler(name);
                if (handler == null)
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Prompt '{name}' not found");
                }

                _logger.LogInformation("Getting prompt: {PromptName}", name);

                var args = promptParams.ContainsKey("arguments") ?
                    JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(promptParams["arguments"])) : null;

                var promptResult = await handler.GetAsync(name, args);

                _logger.LogInformation("Prompt {PromptName} retrieved successfully", name);

                return CreateSuccessResponse(message.Id, promptResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prompt");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Prompt retrieval failed");
            }
        }
    }
}