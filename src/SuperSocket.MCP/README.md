# SuperSocket MCP (Model Context Protocol) Support

This package provides comprehensive support for the Model Context Protocol (MCP) in SuperSocket, enabling LLM applications to communicate with external tools and data sources through a standardized JSON-RPC 2.0 interface.

## Features

- **Complete MCP Protocol Support**: Full JSON-RPC 2.0 implementation following MCP specification version 2024-11-05
- **Tool Integration**: Easy registration and execution of MCP tools with JSON Schema validation
- **Resource Management**: Support for MCP resources with subscription capabilities
- **Prompt Handling**: MCP prompt management and templating
- **SuperSocket Integration**: Leverages SuperSocket's robust pipeline architecture
- **Extensibility**: Clean interfaces for implementing custom handlers
- **Error Handling**: Proper MCP error responses with standard error codes
- **Logging**: Comprehensive logging integration
- **Concurrent Operations**: Thread-safe handler management

## Quick Start

### 1. Create a Simple MCP Server

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Host;

var host = SuperSocketHostBuilder.Create<McpMessage, McpPipelineFilter>(args)
    .UsePackageHandler(async (IAppSession session, McpMessage message) =>
    {
        var logger = session.Server.ServiceProvider.GetService(typeof(ILogger<McpServer>)) as ILogger<McpServer>;
        var serverInfo = new McpServerInfo
        {
            Name = "My MCP Server",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05"
        };
        
        var mcpServer = new McpServer(logger!, serverInfo);
        
        // Register your tools
        mcpServer.RegisterTool("echo", new EchoToolHandler());
        
        // Handle the message
        var response = await mcpServer.HandleMessageAsync(message, session);
        if (response != null)
        {
            await session.SendMcpMessageAsync(response);
        }
    })
    .ConfigureSuperSocket(options =>
    {
        options.Name = "McpServer";
        options.AddListener(new ListenOptions
        {
            Ip = "Any",
            Port = 3000
        });
    })
    .ConfigureLogging((hostCtx, loggingBuilder) =>
    {
        loggingBuilder.AddConsole();
    })
    .Build();

await host.RunAsync();
```

### 2. Implement a Tool Handler

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

```bash
dotnet run --project samples/McpServer/McpServer.csproj
```

The server will start listening on port 3000 and be ready to handle MCP connections.

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

## Extension Methods

The package provides convenient extension methods for session handling:

```csharp
// Send MCP messages
await session.SendMcpMessageAsync(message);
await session.SendMcpResponseAsync(id, result);
await session.SendMcpErrorAsync(id, code, message);
await session.SendMcpNotificationAsync(method, parameters);

// Create MCP messages
var response = McpExtensions.CreateMcpResponse(id, result);
var error = McpExtensions.CreateMcpError(id, code, message);
var notification = McpExtensions.CreateMcpNotification(method, parameters);
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

See the `samples/McpServer/` directory for complete examples including:

- Echo tool: Simple message echoing
- Math tool: Basic arithmetic operations with error handling
- Server setup: Complete MCP server configuration

## Compatibility

- Compatible with MCP specification version 2024-11-05
- Requires .NET 6.0 or later
- Works with SuperSocket's existing infrastructure
- Supports both HTTP and TCP transports

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