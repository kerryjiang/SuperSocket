using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP;

/// <summary>
/// MCP server implementation handling the Model Context Protocol
/// </summary>
public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly McpServerInfo _serverInfo;
    private readonly ConcurrentDictionary<string, IMcpToolHandler> _toolHandlers = new();
    private readonly ConcurrentDictionary<string, IMcpResourceHandler> _resourceHandlers = new();
    private readonly ConcurrentDictionary<string, IMcpPromptHandler> _promptHandlers = new();

    private bool _initialized = false;
    private McpClientInfo? _clientInfo;

    public McpServer(ILogger<McpServer> logger, McpServerInfo serverInfo)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serverInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
    }

    /// <summary>
    /// Registers a tool handler
    /// </summary>
    /// <param name="name">Tool name</param>
    /// <param name="handler">Tool handler</param>
    public void RegisterTool(string name, IMcpToolHandler handler)
    {
        _toolHandlers.TryAdd(name, handler);
        _logger.LogInformation("Registered tool: {ToolName}", name);
    }

    /// <summary>
    /// Registers a resource handler
    /// </summary>
    /// <param name="uri">Resource URI</param>
    /// <param name="handler">Resource handler</param>
    public void RegisterResource(string uri, IMcpResourceHandler handler)
    {
        _resourceHandlers.TryAdd(uri, handler);
        _logger.LogInformation("Registered resource: {ResourceUri}", uri);
    }

    /// <summary>
    /// Registers a prompt handler
    /// </summary>
    /// <param name="name">Prompt name</param>
    /// <param name="handler">Prompt handler</param>
    public void RegisterPrompt(string name, IMcpPromptHandler handler)
    {
        _promptHandlers.TryAdd(name, handler);
        _logger.LogInformation("Registered prompt: {PromptName}", name);
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
            if (message.IsRequest)
            {
                return await HandleRequestAsync(message, session);
            }
            else if (message.IsNotification)
            {
                await HandleNotificationAsync(message, session);
                return null;
            }
            else
            {
                _logger.LogWarning("Received invalid message type");
                return CreateErrorResponse(message.Id, McpErrorCodes.InvalidRequest, "Invalid message type");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MCP message");
            return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Internal server error");
        }
    }

    private async Task<McpMessage> HandleRequestAsync(McpMessage message, IAppSession session)
    {
        return message.Method switch
        {
            "initialize" => await HandleInitializeAsync(message),
            "tools/list" => await HandleListToolsAsync(message),
            "tools/call" => await HandleCallToolAsync(message),
            "resources/list" => await HandleListResourcesAsync(message),
            "resources/read" => await HandleReadResourceAsync(message),
            "prompts/list" => await HandleListPromptsAsync(message),
            "prompts/get" => await HandleGetPromptAsync(message),
            _ => CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Method '{message.Method}' not found")
        };
    }

    private async Task HandleNotificationAsync(McpMessage message, IAppSession session)
    {
        switch (message.Method)
        {
            case "initialized":
                await HandleInitializedAsync(message);
                break;
            case "notifications/cancelled":
                await HandleCancelledAsync(message);
                break;
            default:
                _logger.LogWarning("Received unknown notification: {Method}", message.Method);
                break;
        }
    }

    private async Task<McpMessage> HandleInitializeAsync(McpMessage message)
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

            _clientInfo = initParams.ClientInfo;
            
            var result = new McpInitializeResult
            {
                ProtocolVersion = _serverInfo.ProtocolVersion,
                ServerInfo = _serverInfo,
                Capabilities = new McpServerCapabilities
                {
                    Tools = _toolHandlers.Any() ? new McpToolsCapabilities { ListChanged = true } : null,
                    Resources = _resourceHandlers.Any() ? new McpResourcesCapabilities { ListChanged = true, Subscribe = true } : null,
                    Prompts = _promptHandlers.Any() ? new McpPromptsCapabilities { ListChanged = true } : null,
                    Logging = new McpLoggingCapabilities()
                }
            };

            _logger.LogInformation("Initialized MCP server for client: {ClientName} {ClientVersion}", 
                _clientInfo.Name, _clientInfo.Version);

            return new McpMessage
            {
                Id = message.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initialization");
            return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Initialization failed");
        }
    }

    private async Task HandleInitializedAsync(McpMessage message)
    {
        _initialized = true;
        _logger.LogInformation("MCP server initialization completed");
        await Task.CompletedTask;
    }

    private async Task<McpMessage> HandleListToolsAsync(McpMessage message)
    {
        var tools = new List<McpTool>();
        
        foreach (var handler in _toolHandlers.Values)
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

        return new McpMessage
        {
            Id = message.Id,
            Result = result
        };
    }

    private async Task<McpMessage> HandleCallToolAsync(McpMessage message)
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

            if (!_toolHandlers.TryGetValue(callParams.Name, out var handler))
            {
                return CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Tool '{callParams.Name}' not found");
            }

            var toolResult = await handler.ExecuteAsync(callParams.Arguments ?? new Dictionary<string, object>());

            var result = new McpCallToolResult
            {
                Content = toolResult.Content,
                IsError = toolResult.IsError
            };

            return new McpMessage
            {
                Id = message.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool");
            return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Tool execution failed");
        }
    }

    private async Task<McpMessage> HandleListResourcesAsync(McpMessage message)
    {
        var resources = new List<McpResource>();
        
        foreach (var handler in _resourceHandlers.Values)
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

        return new McpMessage
        {
            Id = message.Id,
            Result = new { resources }
        };
    }

    private async Task<McpMessage> HandleReadResourceAsync(McpMessage message)
    {
        try
        {
            var readParams = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(message.Params),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (readParams == null || !readParams.TryGetValue("uri", out var uriObj))
            {
                return CreateErrorResponse(message.Id, McpErrorCodes.InvalidParams, "URI parameter required");
            }

            var uri = uriObj?.ToString();
            if (string.IsNullOrEmpty(uri) || !_resourceHandlers.TryGetValue(uri, out var handler))
            {
                return CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Resource '{uri}' not found");
            }

            var content = await handler.ReadAsync(uri);

            return new McpMessage
            {
                Id = message.Id,
                Result = new { contents = new[] { content } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading resource");
            return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Resource read failed");
        }
    }

    private async Task<McpMessage> HandleListPromptsAsync(McpMessage message)
    {
        var prompts = new List<McpPrompt>();
        
        foreach (var handler in _promptHandlers.Values)
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

        return new McpMessage
        {
            Id = message.Id,
            Result = new { prompts }
        };
    }

    private async Task<McpMessage> HandleGetPromptAsync(McpMessage message)
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
            if (string.IsNullOrEmpty(name) || !_promptHandlers.TryGetValue(name, out var handler))
            {
                return CreateErrorResponse(message.Id, McpErrorCodes.MethodNotFound, $"Prompt '{name}' not found");
            }

            var args = promptParams.ContainsKey("arguments") ? 
                JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(promptParams["arguments"])) : null;

            var promptResult = await handler.GetAsync(name, args);

            return new McpMessage
            {
                Id = message.Id,
                Result = promptResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt");
            return CreateErrorResponse(message.Id, McpErrorCodes.InternalError, "Prompt retrieval failed");
        }
    }

    private async Task HandleCancelledAsync(McpMessage message)
    {
        // Handle cancellation notifications
        _logger.LogInformation("Received cancellation notification");
        await Task.CompletedTask;
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