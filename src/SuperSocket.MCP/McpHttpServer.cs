using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Http;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Commands;
using SuperSocket.MCP.Models;
using SuperSocket.MCP.Extensions;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP;

/// <summary>
/// HTTP-based MCP server that handles MCP messages over HTTP
/// [Obsolete] Use SuperSocket host builder with UseCommand for new implementations
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
    /// Handles MCP messages using SuperSocket's native command system
    /// [Obsolete] Use SuperSocket host builder with UseCommand for proper command handling
    /// </summary>
    /// <param name="message">MCP message</param>
    /// <param name="session">Session</param>
    /// <returns>Response message</returns>
    [Obsolete("Use SuperSocket host builder with UseCommand for proper command handling")]
    private async Task<McpMessage?> HandleMcpMessageAsync(McpMessage message, IAppSession session)
    {
        // This is a legacy method - new implementations should use SuperSocket's command system
        _logger.LogWarning("Using obsolete HandleMcpMessageAsync method. Consider using SuperSocket host builder with UseCommand.");
        
        // For legacy support, return null as commands should be handled by SuperSocket's command system
        await Task.Yield();
        return null;
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
}