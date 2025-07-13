using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket.Command;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP.Commands
{
    /// <summary>
    /// Dispatcher for MCP commands that can handle both TCP and HTTP messages
    /// </summary>
    public class McpCommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<McpCommandDispatcher> _logger;
        private readonly Dictionary<string, Type> _commandTypes;

        /// <summary>
        /// Initializes a new instance of the McpCommandDispatcher class
        /// </summary>
        /// <param name="serviceProvider">Service provider for dependency injection</param>
        /// <param name="logger">Logger instance</param>
        public McpCommandDispatcher(IServiceProvider serviceProvider, ILogger<McpCommandDispatcher> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandTypes = new Dictionary<string, Type>();
            
            // Register command types
            RegisterCommandTypes();
        }

        /// <summary>
        /// Registers MCP command types with their method names
        /// </summary>
        private void RegisterCommandTypes()
        {
            _commandTypes["initialize"] = typeof(InitializeCommand);
            _commandTypes["tools/list"] = typeof(ListToolsCommand);
            _commandTypes["tools/call"] = typeof(CallToolCommand);
            _commandTypes["resources/list"] = typeof(ListResourcesCommand);
            _commandTypes["resources/read"] = typeof(ReadResourceCommand);
            _commandTypes["prompts/list"] = typeof(ListPromptsCommand);
            _commandTypes["prompts/get"] = typeof(GetPromptCommand);
        }

        /// <summary>
        /// Dispatches an MCP message to the appropriate command
        /// </summary>
        /// <param name="session">Application session</param>
        /// <param name="message">MCP message to dispatch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message, or null if no response should be sent</returns>
        public async Task<McpMessage?> DispatchAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                if (message.IsRequest)
                {
                    return await HandleRequestAsync(session, message, cancellationToken);
                }
                else if (message.IsNotification)
                {
                    await HandleNotificationAsync(session, message, cancellationToken);
                    return null;
                }
                else
                {
                    _logger.LogWarning("Received invalid message type");
                    return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.InvalidRequest, "Invalid message type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching MCP message");
                return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.InternalError, "Internal server error");
            }
        }

        /// <summary>
        /// Handles MCP requests by dispatching to appropriate command
        /// </summary>
        /// <param name="session">Application session</param>
        /// <param name="message">Request message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response message</returns>
        private async Task<McpMessage> HandleRequestAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(message.Method))
            {
                return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.InvalidRequest, "Method is required");
            }

            if (!_commandTypes.TryGetValue(message.Method, out var commandType))
            {
                return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.MethodNotFound, $"Method '{message.Method}' not found");
            }

            try
            {
                var command = _serviceProvider.GetService(commandType) as McpCommandBase;
                if (command == null)
                {
                    _logger.LogError("Could not resolve command type: {CommandType}", commandType.Name);
                    return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.InternalError, "Command not available");
                }

                return await command.HandleMessageAsync(session, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command for method: {Method}", message.Method);
                return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.InternalError, "Command execution failed");
            }
        }

        /// <summary>
        /// Handles MCP notifications
        /// </summary>
        /// <param name="session">Application session</param>
        /// <param name="message">Notification message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task HandleNotificationAsync(IAppSession session, McpMessage message, CancellationToken cancellationToken)
        {
            await Task.Yield(); // Simulate async operation
            
            _logger.LogInformation("Received notification: {Method}", message.Method);
            
            // Handle notification based on method
            switch (message.Method)
            {
                case "initialized":
                    _logger.LogInformation("Client initialized");
                    break;
                case "notifications/cancelled":
                    _logger.LogInformation("Operation cancelled");
                    break;
                default:
                    _logger.LogWarning("Unknown notification method: {Method}", message.Method);
                    break;
            }
        }
    }
}