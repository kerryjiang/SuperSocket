using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Commands;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP;

/// <summary>
/// MCP server implementation handling the Model Context Protocol
/// [Obsolete] Consider using McpCommandServer for new implementations
/// </summary>
public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly McpServerInfo _serverInfo;
    private readonly IMcpHandlerRegistry _handlerRegistry;
    private readonly McpCommandDispatcher _commandDispatcher;

    private bool _initialized = false;
    private McpClientInfo? _clientInfo;

    /// <summary>
    /// Initializes a new instance of the McpServer class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="serverInfo">Server information</param>
    /// <param name="handlerRegistry">Handler registry (optional, will create new if not provided)</param>
    /// <param name="commandDispatcher">Command dispatcher (optional, will create new if not provided)</param>
    public McpServer(ILogger<McpServer> logger, McpServerInfo serverInfo, IMcpHandlerRegistry? handlerRegistry = null, McpCommandDispatcher? commandDispatcher = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serverInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
        _handlerRegistry = handlerRegistry ?? new McpHandlerRegistry(Microsoft.Extensions.Logging.Abstractions.NullLogger<McpHandlerRegistry>.Instance);
        
        if (commandDispatcher == null)
        {
            // Create a simple service provider for fallback
            var services = new ServiceCollection();
            services.AddSingleton(serverInfo);
            services.AddSingleton<IMcpHandlerRegistry>(_handlerRegistry);
            services.AddScoped<InitializeCommand>();
            services.AddScoped<ListToolsCommand>();
            services.AddScoped<CallToolCommand>();
            services.AddScoped<ListResourcesCommand>();
            services.AddScoped<ReadResourceCommand>();
            services.AddScoped<ListPromptsCommand>();
            services.AddScoped<GetPromptCommand>();
            var serviceProvider = services.BuildServiceProvider();
            _commandDispatcher = new McpCommandDispatcher(serviceProvider, Microsoft.Extensions.Logging.Abstractions.NullLogger<McpCommandDispatcher>.Instance);
        }
        else
        {
            _commandDispatcher = commandDispatcher;
        }
    }

    /// <summary>
    /// Registers a tool handler
    /// </summary>
    /// <param name="name">Tool name</param>
    /// <param name="handler">Tool handler</param>
    public void RegisterTool(string name, IMcpToolHandler handler)
    {
        _handlerRegistry.RegisterTool(name, handler);
    }

    /// <summary>
    /// Registers a resource handler
    /// </summary>
    /// <param name="uri">Resource URI</param>
    /// <param name="handler">Resource handler</param>
    public void RegisterResource(string uri, IMcpResourceHandler handler)
    {
        _handlerRegistry.RegisterResource(uri, handler);
    }

    /// <summary>
    /// Registers a prompt handler
    /// </summary>
    /// <param name="name">Prompt name</param>
    /// <param name="handler">Prompt handler</param>
    public void RegisterPrompt(string name, IMcpPromptHandler handler)
    {
        _handlerRegistry.RegisterPrompt(name, handler);
    }

    /// <summary>
    /// Handles an MCP message and returns the appropriate response
    /// </summary>
    /// <param name="message">Incoming MCP message</param>
    /// <param name="session">Session context</param>
    /// <returns>Response message if applicable</returns>
    public async Task<McpMessage?> HandleMessageAsync(McpMessage message, IAppSession session)
    {
        try
        {
            // Use the command dispatcher to handle the message
            var response = await _commandDispatcher.DispatchAsync(session, message);
            
            // Track initialization state for initialize/initialized messages
            if (message.Method == "initialize" && response != null && response.Error == null)
            {
                // Extract client info from initialize parameters
                var initParams = JsonSerializer.Deserialize<McpInitializeParams>(
                    JsonSerializer.Serialize(message.Params), 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (initParams != null)
                {
                    _clientInfo = initParams.ClientInfo;
                    _logger.LogInformation("Initialized MCP server for client: {ClientName} {ClientVersion}", 
                        _clientInfo.Name, _clientInfo.Version);
                }
            }
            else if (message.Method == "initialized")
            {
                _initialized = true;
                _logger.LogInformation("MCP server initialization completed");
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MCP message");
            return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Internal server error");
        }
    }

    private static McpMessage CreateErrorResponse(object? id, int code, string message)
    {
        return new McpMessage
        {
            Id = id,
            Error = new McpError
            {
                Code = code,
                Message = message
            }
        };
    }
}