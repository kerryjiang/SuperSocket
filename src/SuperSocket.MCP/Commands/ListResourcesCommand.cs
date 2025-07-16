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
    /// Command to handle MCP resources/list requests
    /// </summary>
    [Command("resources/list")]
    public class ListResourcesCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the ListResourcesCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public ListResourcesCommand(ILogger<ListResourcesCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var resources = new List<McpResource>();
                var resourceHandlers = _handlerRegistry.GetResourceHandlers();

                foreach (var handler in resourceHandlers.Values)
                {
                    try
                    {
                        var resource = await handler.GetResourceDefinitionAsync();
                        resources.Add(resource);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting resource definition");
                    }
                }

                var result = new { resources };

                _logger.LogDebug("Listed {ResourceCount} resources", resources.Count);

                return CreateSuccessResponse(message.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing resources");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Failed to list resources");
            }
        }
    }
}