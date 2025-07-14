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
    /// Command to handle MCP initialized notifications
    /// </summary>
    [Command("initialized")]
    public class InitializedCommand : McpCommandBase
    {
        /// <summary>
        /// Initializes a new instance of the InitializedCommand class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="handlerRegistry">Handler registry</param>
        public InitializedCommand(ILogger<InitializedCommand> logger, IMcpHandlerRegistry handlerRegistry)
            : base(logger, handlerRegistry)
        {
        }

        /// <inheritdoc />
        protected override async Task<McpMessage?> HandleAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            await Task.Yield(); // Simulate async operation
            
            _logger.LogInformation("MCP server initialization completed");
            
            // Notifications don't send responses
            return null;
        }
    }
}