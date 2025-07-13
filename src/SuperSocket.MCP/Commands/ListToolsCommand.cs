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
    /// Command to handle MCP tools/list requests
    /// </summary>
    [Command("tools/list")]
    public class ListToolsCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the ListToolsCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public ListToolsCommand(ILogger<ListToolsCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var tools = new List<McpTool>();
                var toolHandlers = _handlerRegistry.GetToolHandlers();

                foreach (var handler in toolHandlers.Values)
                {
                    try
                    {
                        var tool = await handler.GetToolDefinitionAsync();
                        tools.Add(tool);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting tool definition");
                    }
                }

                var result = new McpListToolsResult
                {
                    Tools = tools
                };

                _logger.LogDebug("Listed {ToolCount} tools", tools.Count);

                return CreateSuccessResponse(message.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing tools");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Failed to list tools");
            }
        }
    }
}