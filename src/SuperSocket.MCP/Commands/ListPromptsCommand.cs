using System;
using System.Collections.Generic;
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
    /// Command to handle MCP prompts/list requests
    /// </summary>
    [Command("prompts/list")]
    public class ListPromptsCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the ListPromptsCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public ListPromptsCommand(ILogger<ListPromptsCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var prompts = new List<McpPrompt>();
                var promptHandlers = _handlerRegistry.GetPromptHandlers();

                foreach (var handler in promptHandlers.Values)
                {
                    try
                    {
                        var prompt = await handler.GetPromptDefinitionAsync();
                        prompts.Add(prompt);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting prompt definition");
                    }
                }

                var result = new { prompts };

                _logger.LogDebug("Listed {PromptCount} prompts", prompts.Count);

                return CreateSuccessResponse(message.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing prompts");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Failed to list prompts");
            }
        }
    }
}