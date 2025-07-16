using System;
using System.Linq;
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
    /// Command to handle MCP initialize requests
    /// </summary>
    [Command("initialize")]
    public class InitializeCommand : McpCommandBase
    {
        private readonly McpServerInfo _serverInfo;

        /// <summary>
        /// Initializes a new instance of the InitializeCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        /// <param name="serverInfo">Server information</param>
        public InitializeCommand(ILogger<InitializeCommand> logger, IMcpHandlerRegistry handlerRegistry, McpServerInfo serverInfo)
            : base(logger, handlerRegistry)
        {
            _serverInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var initParams = JsonSerializer.Deserialize<McpInitializeParams>(
                    JsonSerializer.Serialize(message.Params),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (initParams == null)
                {
                    return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "Invalid initialize parameters");
                }

                _logger.LogInformation("Initializing MCP server for client: {ClientName} {ClientVersion}",
                    initParams.ClientInfo.Name, initParams.ClientInfo.Version);

                // Create server capabilities based on registered handlers
                var toolHandlers = _handlerRegistry.GetToolHandlers();
                var resourceHandlers = _handlerRegistry.GetResourceHandlers();
                var promptHandlers = _handlerRegistry.GetPromptHandlers();

                var capabilities = new McpServerCapabilities
                {
                    Tools = toolHandlers.Any() ? new McpToolsCapabilities { ListChanged = true } : null,
                    Resources = resourceHandlers.Any() ? new McpResourcesCapabilities { ListChanged = true, Subscribe = true } : null,
                    Prompts = promptHandlers.Any() ? new McpPromptsCapabilities { ListChanged = true } : null,
                    Logging = new McpLoggingCapabilities()
                };

                var result = new McpInitializeResult
                {
                    ProtocolVersion = _serverInfo.ProtocolVersion,
                    ServerInfo = _serverInfo,
                    Capabilities = capabilities
                };

                _logger.LogInformation("MCP server initialized successfully");

                return CreateSuccessResponse(message.Id, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MCP initialization");
                return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Initialization failed");
            }
        }
    }
}