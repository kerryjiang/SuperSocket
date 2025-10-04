using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.MCP.Commands;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.WebSocket.Server;
using SuperSocket.WebSocket;
using SuperSocket.Command;

namespace SuperSocket.MCP.Extensions
{
    /// <summary>
    /// Extension methods for configuring MCP over WebSocket connections
    /// </summary>
    public static class McpWebSocketExtensions
    {
        /// <summary>
        /// Configures the host builder to use MCP over WebSocket with automatic command registration
        /// </summary>
        /// <param name="hostBuilder">The WebSocket host builder</param>
        /// <returns>Configured host builder with MCP support</returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseMcp(this ISuperSocketHostBuilder<WebSocketPackage> hostBuilder)
        {
            return hostBuilder.UseCommand<McpMessage, McpWebSocketPipelineFilter>(commandOptions =>
            {
                // Register all standard MCP commands
                commandOptions.AddCommand<InitializeCommand>();
                commandOptions.AddCommand<InitializedCommand>();
                commandOptions.AddCommand<ListToolsCommand>();
                commandOptions.AddCommand<CallToolCommand>();
                commandOptions.AddCommand<ListResourcesCommand>();
                commandOptions.AddCommand<ListPromptsCommand>();
                commandOptions.AddCommand<GetPromptCommand>();
                commandOptions.AddCommand<ReadResourceCommand>();
            });
        }

        /// <summary>
        /// Configures the host builder to use MCP over WebSocket with custom command configuration
        /// </summary>
        /// <param name="hostBuilder">The WebSocket host builder</param>
        /// <param name="configureCommands">Action to configure MCP commands</param>
        /// <returns>Configured host builder with MCP support</returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseMcp(this ISuperSocketHostBuilder<WebSocketPackage> hostBuilder, 
            Action<CommandOptions> configureCommands)
        {
            return hostBuilder.UseCommand<McpMessage, McpWebSocketPipelineFilter>(configureCommands);
        }

        /// <summary>
        /// Configures the host builder to use MCP over WebSocket with a specific protocol name
        /// </summary>
        /// <param name="hostBuilder">The WebSocket host builder</param>
        /// <param name="protocol">The WebSocket sub-protocol name (e.g., "mcp")</param>
        /// <param name="configureCommands">Optional action to configure MCP commands</param>
        /// <returns>Configured host builder with MCP support</returns>
        public static ISuperSocketHostBuilder<WebSocketPackage> UseMcp(this ISuperSocketHostBuilder<WebSocketPackage> hostBuilder,
            string protocol, Action<CommandOptions> configureCommands = null)
        {
            var commandAction = configureCommands ?? (options =>
            {
                // Register all standard MCP commands
                options.AddCommand<InitializeCommand>();
                options.AddCommand<InitializedCommand>();
                options.AddCommand<ListToolsCommand>();
                options.AddCommand<CallToolCommand>();
                options.AddCommand<ListResourcesCommand>();
                options.AddCommand<ListPromptsCommand>();
                options.AddCommand<GetPromptCommand>();
                options.AddCommand<ReadResourceCommand>();
            });

            return hostBuilder.UseCommand<McpMessage, McpWebSocketPipelineFilter>(protocol, commandAction);
        }

        /// <summary>
        /// Sends an MCP message response over WebSocket
        /// </summary>
        /// <param name="session">The WebSocket session</param>
        /// <param name="message">The MCP message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        public static async ValueTask SendMcpMessageAsync(this WebSocketSession session, McpMessage message, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Serialize the MCP message to JSON
                var json = System.Text.Json.JsonSerializer.Serialize(message, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                // Send as WebSocket text frame
                await session.SendAsync(json, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send MCP message over WebSocket: {ex.Message}", ex);
            }
        }
    }
}