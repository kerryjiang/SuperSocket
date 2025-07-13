using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Http;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP;

/// <summary>
/// HTTP-based MCP server that handles MCP messages over HTTP
/// [Obsolete] Consider using McpCommandServer for new implementations
/// </summary>
public class McpHttpServer
{
    private readonly ILogger<McpHttpServer> _logger;
    private readonly McpServerInfo _serverInfo;
    private readonly IMcpHandlerRegistry _handlerRegistry;
    private readonly Dictionary<string, ServerSentEventWriter> _sseClients;

    /// <summary>
    /// Initializes a new instance of the McpHttpServer class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="serverInfo">Server information</param>
    /// <param name="handlerRegistry">Handler registry (optional, will create new if not provided)</param>
    public McpHttpServer(ILogger<McpHttpServer> logger, McpServerInfo serverInfo, IMcpHandlerRegistry? handlerRegistry = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serverInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
        _handlerRegistry = handlerRegistry ?? new McpHandlerRegistry(Microsoft.Extensions.Logging.Abstractions.NullLogger<McpHandlerRegistry>.Instance);
        _sseClients = new Dictionary<string, ServerSentEventWriter>();
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
    /// <param name="name">Resource name</param>
    /// <param name="handler">Resource handler</param>
    public void RegisterResource(string name, IMcpResourceHandler handler)
    {
        _handlerRegistry.RegisterResource(name, handler);
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
    /// Handles HTTP MCP requests
    /// </summary>
    /// <param name="httpRequest">HTTP request</param>
    /// <param name="session">Session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task HandleHttpRequestAsync(McpHttpRequest httpRequest, IAppSession session, CancellationToken cancellationToken = default)
    {
        try
        {
            // Handle different HTTP paths
            var path = httpRequest.HttpRequest.Path;
            
            if (path == "/mcp" && httpRequest.HttpRequest.Method == "POST")
            {
                // Handle MCP JSON-RPC requests
                if (httpRequest.McpMessage != null)
                {
                    var response = await HandleMcpMessageAsync(httpRequest.McpMessage, session);
                    if (response != null)
                    {
                        await session.SendHttpMcpResponseAsync(response, cancellationToken);
                    }
                }
                else
                {
                    await session.SendHttpMcpErrorAsync(null, McpErrorCodes.InvalidRequest, "Invalid MCP message in request body", cancellationToken: cancellationToken);
                }
            }
            else if (path == "/mcp/events" && httpRequest.HttpRequest.Method == "GET")
            {
                // Handle Server-Sent Events connection
                if (httpRequest.HttpRequest.AcceptsEventStream)
                {
                    var writer = await session.StartMcpServerSentEventsAsync(cancellationToken);
                    var clientId = session.SessionID;
                    _sseClients[clientId] = writer;
                    
                    _logger.LogInformation("SSE client connected: {ClientId}", clientId);
                    
                    // Send initial server info
                    await writer.SendMcpNotificationEventAsync("server/info", new { server = _serverInfo }, cancellationToken);
                }
                else
                {
                    var response = new HttpResponse(400, "Bad Request");
                    response.SetContentType("text/plain");
                    response.Body = "This endpoint requires Server-Sent Events support";
                    await session.SendAsync(HttpResponseEncoder.Instance, response, cancellationToken);
                }
            }
            else if (path == "/mcp/capabilities" && httpRequest.HttpRequest.Method == "GET")
            {
                // Handle capability inquiry
                var toolHandlers = _handlerRegistry.GetToolHandlers();
                var resourceHandlers = _handlerRegistry.GetResourceHandlers();
                var promptHandlers = _handlerRegistry.GetPromptHandlers();
                
                var capabilities = new McpServerCapabilities
                {
                    Tools = toolHandlers.Any() ? new McpToolsCapabilities { ListChanged = true } : null,
                    Resources = resourceHandlers.Any() ? new McpResourcesCapabilities { Subscribe = true, ListChanged = true } : null,
                    Prompts = promptHandlers.Any() ? new McpPromptsCapabilities { ListChanged = true } : null,
                    Logging = new McpLoggingCapabilities()
                };

                var response = new HttpResponse();
                response.SetContentType("application/json");
                response.Body = System.Text.Json.JsonSerializer.Serialize(capabilities);
                await session.SendAsync(HttpResponseEncoder.Instance, response, cancellationToken);
            }
            else
            {
                var response = new HttpResponse(404, "Not Found");
                response.SetContentType("text/plain");
                response.Body = "MCP endpoint not found";
                await session.SendAsync(HttpResponseEncoder.Instance, response, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling HTTP MCP request");
            await session.SendHttpMcpErrorAsync(null, McpErrorCodes.InternalError, ex.Message, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Handles MCP messages using the existing logic from McpServer
    /// </summary>
    /// <param name="message">MCP message</param>
    /// <param name="session">Session</param>
    /// <returns>Response message</returns>
    private async Task<McpMessage> HandleMcpMessageAsync(McpMessage message, IAppSession session)
    {
        if (message.IsRequest)
        {
            return await HandleMcpRequestAsync(message, session);
        }
        else if (message.IsNotification)
        {
            await HandleMcpNotificationAsync(message, session);
            return null; // No response for notifications
        }
        else
        {
            return McpExtensions.CreateMcpError(message.Id, McpErrorCodes.InvalidRequest, "Invalid message type");
        }
    }

    /// <summary>
    /// Handles MCP requests
    /// </summary>
    /// <param name="request">Request message</param>
    /// <param name="session">Session</param>
    /// <returns>Response message</returns>
    private async Task<McpMessage> HandleMcpRequestAsync(McpMessage request, IAppSession session)
    {
        try
        {
            return request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request),
                "tools/list" => await HandleToolsListAsync(request),
                "tools/call" => await HandleToolsCallAsync(request),
                "resources/list" => await HandleResourcesListAsync(request),
                "resources/read" => await HandleResourcesReadAsync(request),
                "prompts/list" => await HandlePromptsListAsync(request),
                "prompts/get" => await HandlePromptsGetAsync(request),
                _ => McpExtensions.CreateMcpError(request.Id, McpErrorCodes.MethodNotFound, $"Method '{request.Method}' not found")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MCP request: {Method}", request.Method);
            return McpExtensions.CreateMcpError(request.Id, McpErrorCodes.InternalError, ex.Message);
        }
    }

    /// <summary>
    /// Handles MCP notifications
    /// </summary>
    /// <param name="notification">Notification message</param>
    /// <param name="session">Session</param>
    /// <returns>Task</returns>
    private async Task HandleMcpNotificationAsync(McpMessage notification, IAppSession session)
    {
        await Task.Yield(); // Simulate async operation
        
        _logger.LogInformation("Received notification: {Method}", notification.Method);
        
        // Handle notification based on method
        switch (notification.Method)
        {
            case "initialized":
                _logger.LogInformation("Client initialized");
                break;
            case "notifications/cancelled":
                _logger.LogInformation("Operation cancelled");
                break;
            default:
                _logger.LogWarning("Unknown notification method: {Method}", notification.Method);
                break;
        }
    }

    /// <summary>
    /// Broadcasts a notification to all SSE clients
    /// </summary>
    /// <param name="method">Notification method</param>
    /// <param name="parameters">Notification parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task BroadcastNotificationAsync(string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var disconnectedClients = new List<string>();
        
        foreach (var kvp in _sseClients)
        {
            try
            {
                await kvp.Value.SendMcpNotificationEventAsync(method, parameters, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification to client {ClientId}", kvp.Key);
                disconnectedClients.Add(kvp.Key);
            }
        }
        
        // Remove disconnected clients
        foreach (var clientId in disconnectedClients)
        {
            _sseClients.Remove(clientId);
        }
    }

    // Implementation of existing MCP methods (copied from McpServer)
    
    private async Task<McpMessage> HandleInitializeAsync(McpMessage request)
    {
        await Task.Yield(); // Simulate async operation

        var initializeRequest = System.Text.Json.JsonSerializer.Deserialize<McpInitializeParams>(
            System.Text.Json.JsonSerializer.Serialize(request.Params));

        var toolHandlers = _handlerRegistry.GetToolHandlers();
        var resourceHandlers = _handlerRegistry.GetResourceHandlers();
        var promptHandlers = _handlerRegistry.GetPromptHandlers();

        var response = new McpInitializeResult
        {
            ProtocolVersion = _serverInfo.ProtocolVersion,
            ServerInfo = _serverInfo,
            Capabilities = new McpServerCapabilities
            {
                Tools = toolHandlers.Any() ? new McpToolsCapabilities { ListChanged = true } : null,
                Resources = resourceHandlers.Any() ? new McpResourcesCapabilities { Subscribe = true, ListChanged = true } : null,
                Prompts = promptHandlers.Any() ? new McpPromptsCapabilities { ListChanged = true } : null,
                Logging = new McpLoggingCapabilities()
            }
        };

        return McpExtensions.CreateMcpResponse(request.Id, response);
    }

    private async Task<McpMessage> HandleToolsListAsync(McpMessage request)
    {
        var tools = new List<McpTool>();
        var toolHandlers = _handlerRegistry.GetToolHandlers();
        
        foreach (var kvp in toolHandlers)
        {
            var tool = await kvp.Value.GetToolDefinitionAsync();
            tools.Add(tool);
        }

        var response = new McpToolsListResponse { Tools = tools };
        return McpExtensions.CreateMcpResponse(request.Id, response);
    }

    private async Task<McpMessage> HandleToolsCallAsync(McpMessage request)
    {
        var callRequest = System.Text.Json.JsonSerializer.Deserialize<McpToolsCallRequest>(
            System.Text.Json.JsonSerializer.Serialize(request.Params));

        var handler = _handlerRegistry.GetToolHandler(callRequest.Name);
        if (handler == null)
        {
            return McpExtensions.CreateMcpError(request.Id, McpErrorCodes.MethodNotFound, 
                $"Tool '{callRequest.Name}' not found");
        }

        var result = await handler.ExecuteAsync(callRequest.Arguments);
        return McpExtensions.CreateMcpResponse(request.Id, result);
    }

    private async Task<McpMessage> HandleResourcesListAsync(McpMessage request)
    {
        var resources = new List<McpResource>();
        var resourceHandlers = _handlerRegistry.GetResourceHandlers();
        
        foreach (var kvp in resourceHandlers)
        {
            var resource = await kvp.Value.GetResourceDefinitionAsync();
            resources.Add(resource);
        }

        var response = new McpResourcesListResponse { Resources = resources };
        return McpExtensions.CreateMcpResponse(request.Id, response);
    }

    private async Task<McpMessage> HandleResourcesReadAsync(McpMessage request)
    {
        var readRequest = System.Text.Json.JsonSerializer.Deserialize<McpResourcesReadRequest>(
            System.Text.Json.JsonSerializer.Serialize(request.Params));

        var handler = _handlerRegistry.GetResourceHandler(readRequest.Uri);
        if (handler == null)
        {
            return McpExtensions.CreateMcpError(request.Id, McpErrorCodes.MethodNotFound, 
                $"Resource '{readRequest.Uri}' not found");
        }

        var result = await handler.ReadAsync(readRequest.Uri);
        return McpExtensions.CreateMcpResponse(request.Id, result);
    }

    private async Task<McpMessage> HandlePromptsListAsync(McpMessage request)
    {
        var prompts = new List<McpPrompt>();
        var promptHandlers = _handlerRegistry.GetPromptHandlers();
        
        foreach (var kvp in promptHandlers)
        {
            var prompt = await kvp.Value.GetPromptDefinitionAsync();
            prompts.Add(prompt);
        }

        var response = new McpPromptsListResponse { Prompts = prompts };
        return McpExtensions.CreateMcpResponse(request.Id, response);
    }

    private async Task<McpMessage> HandlePromptsGetAsync(McpMessage request)
    {
        var getRequest = System.Text.Json.JsonSerializer.Deserialize<McpPromptsGetRequest>(
            System.Text.Json.JsonSerializer.Serialize(request.Params));

        var handler = _handlerRegistry.GetPromptHandler(getRequest.Name);
        if (handler == null)
        {
            return McpExtensions.CreateMcpError(request.Id, McpErrorCodes.MethodNotFound, 
                $"Prompt '{getRequest.Name}' not found");
        }

        var result = await handler.GetAsync(getRequest.Name, getRequest.Arguments);
        return McpExtensions.CreateMcpResponse(request.Id, result);
    }
}