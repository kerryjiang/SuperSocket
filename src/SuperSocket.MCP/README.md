# SuperSocket MCP (Model Context Protocol) Support

This package provides comprehensive support for the Model Context Protocol (MCP) in SuperSocket, enabling LLM applications to communicate with external tools and data sources through a standardized JSON-RPC 2.0 interface.

## Features

- **Complete MCP Protocol Support**: Full JSON-RPC 2.0 implementation following MCP specification version 2024-11-05
- **Multiple Transport Support**: Stdio, HTTP (POST/SSE), and WebSocket transports
- **Tool Integration**: Easy registration and execution of MCP tools with JSON Schema validation
- **Resource Management**: Support for MCP resources with subscription capabilities
- **Prompt Handling**: MCP prompt management and templating
- **Server-Sent Events**: Real-time notifications via SSE for HTTP transport
- **SuperSocket Integration**: Leverages SuperSocket's robust pipeline architecture
- **Extensibility**: Clean interfaces for implementing custom handlers
- **Error Handling**: Proper MCP error responses with standard error codes
- **Logging**: Comprehensive logging integration
- **Concurrent Operations**: Thread-safe handler management

## Transport Options

### 1. Stdio Transport (Recommended)

The stdio transport is the most common way to use MCP, especially for integration with AI assistants and command-line tools. It uses standard input/output for communication.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureMcp(mcpBuilder =>
    {
        mcpBuilder
            .ConfigureServer(serverInfo =>
            {
                serverInfo.Name = "My MCP Server";
                serverInfo.Version = "1.0.0";
                serverInfo.ProtocolVersion = "2024-11-05";
            })
            .AddTool<EchoToolHandler>()
            .AddTool<MathToolHandler>();
    })
    .Build();

await host.RunAsync();
```

### 2. WebSocket Transport

WebSocket transport allows web browsers and WebSocket clients to connect directly to MCP servers, enabling web-based integrations and real-time communication.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.WebSocket.Server;

var host = WebSocketHostBuilder.Create(args)
    .ConfigureServices((hostCtx, services) =>
    {
        // Register MCP services
        services.AddSingleton<IMcpHandlerRegistry, McpHandlerRegistry>();
        services.AddSingleton<McpServerInfo>(new McpServerInfo
        {
            Name = "SuperSocket MCP WebSocket Server",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05"
        });
        
        // Register sample tools
        services.AddSingleton<IMcpToolHandler, EchoToolHandler>();
        services.AddSingleton<IMcpToolHandler, MathToolHandler>();
        services.AddSingleton<IMcpToolHandler, TimeToolHandler>();
    })
    .UseMcp("mcp") // Use MCP with "mcp" sub-protocol
    .ConfigureLogging((hostCtx, loggingBuilder) =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.RunAsync();
```

### 3. HTTP Transport

HTTP transport provides REST API endpoints and Server-Sent Events for web-compatible MCP communication.

```csharp
using SuperSocket.MCP;
using SuperSocket.Server.Host;

var host = SuperSocketHostBuilder.Create<McpHttpRequest, McpHttpPipelineFilter>(args)
    .UsePackageHandler(async (session, request) =>
    {
        var logger = session.Server.ServiceProvider.GetService(typeof(ILogger<McpHttpServer>)) as ILogger<McpHttpServer>;
        var serverInfo = new McpServerInfo
        {
            Name = "My HTTP MCP Server",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05"
        };
        
        var mcpServer = new McpHttpServer(logger!, serverInfo);
        
        // Register your tools
        mcpServer.RegisterTool("echo", new EchoToolHandler());
        
        // Handle HTTP request
        await mcpServer.HandleHttpRequestAsync(request, session);
    })
    .ConfigureSuperSocket(options =>
    {
        options.Name = "McpHttpServer";
        options.AddListener(new ListenOptions { Ip = "Any", Port = 8080 });
    })
    .Build();

await host.RunAsync();
```

```csharp
using SuperSocket.MCP;
using SuperSocket.Server.Host;

var host = SuperSocketHostBuilder.Create<McpHttpRequest, McpHttpPipelineFilter>(args)
    .UsePackageHandler(async (session, request) =>
    {
        var logger = session.Server.ServiceProvider.GetService(typeof(ILogger<McpHttpServer>)) as ILogger<McpHttpServer>;
        var serverInfo = new McpServerInfo
        {
            Name = "My HTTP MCP Server",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05"
        };
        
        var mcpServer = new McpHttpServer(logger!, serverInfo);
        
        // Register your tools
        mcpServer.RegisterTool("echo", new EchoToolHandler());
        
        // Handle HTTP request
        await mcpServer.HandleHttpRequestAsync(request, session);
    })
    .ConfigureSuperSocket(options =>
    {
        options.Name = "McpHttpServer";
        options.AddListener(new ListenOptions { Ip = "Any", Port = 8080 });
    })
    .Build();

await host.RunAsync();
```

## Usage Examples

### Stdio Transport

Run a stdio MCP server:

```bash
# Start the server
dotnet run --project samples/McpStdioServer/

# Connect with your MCP client
# The server reads from stdin and writes to stdout
```

### WebSocket Transport

Connect to WebSocket MCP server from browser:

```javascript
// Connect with WebSocket sub-protocol "mcp"
const ws = new WebSocket('ws://localhost:8080', ['mcp']);

ws.onopen = () => {
    // Send initialize message
    ws.send(JSON.stringify({
        jsonrpc: "2.0",
        id: 1,
        method: "initialize",
        params: {
            protocolVersion: "2024-11-05",
            capabilities: {},
            clientInfo: { name: "Browser Client", version: "1.0.0" }
        }
    }));
};

ws.onmessage = (event) => {
    const response = JSON.parse(event.data);
    console.log('MCP Response:', response);
};
```

### HTTP Transport API Endpoints

- **POST /mcp**: Send MCP JSON-RPC requests
- **GET /mcp/events**: Connect to Server-Sent Events for notifications
- **GET /mcp/capabilities**: Get server capabilities

Example HTTP usage:

```bash
# Send a tool call via HTTP POST
curl -X POST http://localhost:8080/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "echo",
      "arguments": {
        "message": "Hello from HTTP!"
      }
    }
  }'

# Connect to SSE for real-time notifications
curl -N -H "Accept: text/event-stream" http://localhost:8080/mcp/events
```

## Tool Handler Implementation

### Implement a Tool Handler

```csharp
public class EchoToolHandler : IMcpToolHandler
{
    public Task<McpTool> GetToolDefinitionAsync()
    {
        return Task.FromResult(new McpTool
        {
            Name = "echo",
            Description = "Echo back the input message",
            InputSchema = new
            {
                type = "object",
                properties = new
                {
                    message = new { type = "string", description = "Message to echo back" }
                },
                required = new[] { "message" }
            }
        });
    }

    public Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments)
    {
        var message = arguments.TryGetValue("message", out var msg) ? msg.ToString() : "Hello!";
        return Task.FromResult(new McpToolResult
        {
            Content = new List<McpContent>
            {
                new McpContent { Type = "text", Text = $"Echo: {message}" }
            }
        });
    }
}
```

### 3. Run the Server

For stdio transport:
```bash
dotnet run --project samples/McpStdioServer/McpStdioServer.csproj
```

For WebSocket transport:
```bash
dotnet run --project samples/McpWebSocketServer/McpWebSocketServer.csproj
```

For HTTP transport:
```bash
dotnet run --project samples/McpHttpServer/McpHttpServer.csproj
```

The servers will be ready to handle MCP connections via their respective transports.

## Architecture

### Core Components

1. **McpMessage**: JSON-RPC 2.0 message structure
2. **McpPipelineFilter**: Handles Content-Length header format parsing
3. **McpServer**: Complete MCP protocol implementation
4. **Handler Interfaces**: Clean abstractions for tools, resources, and prompts
5. **Extensions**: Convenient methods for message sending and handling

### Protocol Support

The implementation supports all major MCP protocol features:

- **Initialization**: Server/client handshake with capability negotiation
- **Tools**: Registration and execution of callable tools
- **Resources**: Management of readable resources
- **Prompts**: Template-based prompt handling
- **Notifications**: Asynchronous message delivery
- **Error Handling**: Standard JSON-RPC 2.0 error responses

## Handler Interfaces

### IMcpToolHandler

Implement this interface to create MCP tools:

```csharp
public interface IMcpToolHandler
{
    Task<McpTool> GetToolDefinitionAsync();
    Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments);
}
```

### IMcpResourceHandler

Implement this interface to create MCP resources:

```csharp
public interface IMcpResourceHandler
{
    Task<McpResource> GetResourceDefinitionAsync();
    Task<McpResourceContent> ReadAsync(string uri);
}
```

### IMcpPromptHandler

Implement this interface to create MCP prompts:

```csharp
public interface IMcpPromptHandler
{
    Task<McpPrompt> GetPromptDefinitionAsync();
    Task<McpPromptResult> GetPromptAsync(Dictionary<string, object>? arguments = null);
}
```

### Extensions for Different Transports

The package provides convenient extension methods for session handling:

### Stdio Transport Extensions

```csharp
// For stdio transport, use standard console I/O
// Messages are automatically handled through the console pipeline
```

### WebSocket Transport Extensions

```csharp
// Send MCP messages over WebSocket
await session.SendMcpMessageAsync(message);

// WebSocket-specific helpers
await session.SendAsync(jsonMessage); // Send raw JSON string
```

### HTTP Transport Extensions

```csharp
// Send HTTP responses with MCP content
await session.SendHttpMcpResponseAsync(message);
await session.SendHttpMcpErrorAsync(id, code, message);
await session.SendHttpMcpResultAsync(id, result);

// Server-Sent Events support
var writer = await session.StartMcpServerSentEventsAsync();
await writer.SendMcpEventAsync(message);
await writer.SendMcpNotificationEventAsync(method, parameters);
```

## Testing

The package includes comprehensive tests covering:

- Message serialization/deserialization
- Tool handler execution
- Protocol compliance
- Error handling

Run tests with:

```bash
dotnet test test/SuperSocket.MCP.Tests/
```

## Examples

See the sample directories for complete examples:

- **samples/McpStdioServer/**: Stdio-based MCP server (recommended for most use cases)
- **samples/McpWebSocketServer/**: WebSocket-based MCP server with browser support
- **samples/McpHttpServer/**: HTTP-based MCP server with REST API and SSE support

All examples include:
- Echo tool: Simple message echoing
- Math tool: Basic arithmetic operations with error handling
- Complete server setup and configuration

## Transport Comparison

| Feature | Stdio | WebSocket | HTTP (POST/SSE) |
|---------|-------|-----------|-----------------|
| Protocol | JSON-RPC over stdio | JSON-RPC over WebSocket | HTTP + JSON-RPC |
| Use Case | CLI tools, AI assistants | Web apps, real-time apps | REST APIs, web services |
| Bidirectional | Yes | Yes | SSE for notifications |
| Web Compatible | No | Yes | Yes |
| Firewall Friendly | N/A | Yes | Yes |
| Load Balancer Support | N/A | Full | Full |
| Browser Support | No | Yes | Yes |
| Default Port | N/A | 8080 | 8080 |
| Best For | Command line integration | Interactive web apps | HTTP-based integrations |

All transports support the same MCP features and can be used based on your specific requirements.

## Compatibility

- Compatible with MCP specification version 2024-11-05
- Requires .NET 6.0 or later
- Works with SuperSocket's existing infrastructure
- Supports stdio, WebSocket, and HTTP transports
- Stdio transport is the recommended approach for most MCP integrations
- WebSocket transport enables browser-based MCP clients
- HTTP transport uses standard HTTP methods with JSON-RPC 2.0 bodies

## Error Handling

The implementation provides comprehensive error handling with standard JSON-RPC 2.0 error codes:

- **Parse Error (-32700)**: Invalid JSON
- **Invalid Request (-32600)**: Invalid request structure
- **Method Not Found (-32601)**: Unknown method
- **Invalid Params (-32602)**: Invalid parameters
- **Internal Error (-32603)**: Internal server error
- **Server Error (-32000)**: Application-specific errors

## Contributing

When contributing to the MCP implementation:

1. Follow the existing code patterns and style
2. Add appropriate tests for new features
3. Update documentation for API changes
4. Ensure compatibility with the MCP specification

## License

This package is part of SuperSocket and follows the same Apache 2.0 license.