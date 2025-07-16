# MCP SuperSocket Commands Usage

This document shows how to use the refactored MCP implementation with SuperSocket's native command system.

## Overview

The MCP implementation now uses SuperSocket's native command system instead of a custom dispatcher. This provides better integration with SuperSocket's architecture and removes code duplication.

## Key Changes

1. **Removed Custom Dispatcher**: The `McpCommandDispatcher` has been simplified to just a notification handler
2. **Native Command System**: Commands now work directly with SuperSocket's `IAsyncCommand<McpMessage>` interface
3. **Proper Registration**: Commands are registered using SuperSocket's `UseCommand` method
4. **Unified Architecture**: Both TCP and HTTP transports use the same command system

## Usage Examples

### Basic TCP MCP Server

```csharp
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Host;

var serverInfo = new McpServerInfo
{
    Name = "MyMcpServer",
    Version = "1.0.0"
};

var host = SuperSocketHostBuilder.Create<McpMessage, McpPipelineFilter>()
    .UseMcpCommands(serverInfo)  // This configures all MCP commands
    .ConfigureAppConfiguration((hostCtx, configApp) =>
    {
        configApp.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "serverOptions:name", "McpServer" },
            { "serverOptions:listeners:0:ip", "Any" },
            { "serverOptions:listeners:0:port", "8080" }
        });
    })
    .ConfigureLogging((hostCtx, loggingBuilder) =>
    {
        loggingBuilder.AddConsole();
    })
    .Build();

await host.RunAsync();
```

### HTTP MCP Server

```csharp
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Host;

var serverInfo = new McpServerInfo
{
    Name = "MyMcpHttpServer",
    Version = "1.0.0"
};

var host = SuperSocketHostBuilder.Create<McpHttpRequest, McpHttpPipelineFilter>()
    .UseMcpCommands(serverInfo)  // Same command registration
    .ConfigureAppConfiguration((hostCtx, configApp) =>
    {
        configApp.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "serverOptions:name", "McpHttpServer" },
            { "serverOptions:listeners:0:ip", "Any" },
            { "serverOptions:listeners:0:port", "8080" }
        });
    })
    .Build();

await host.RunAsync();
```

### Manual Command Registration

If you need more control over command registration:

```csharp
var host = SuperSocketHostBuilder.Create<McpMessage, McpPipelineFilter>()
    .ConfigureServices((hostCtx, services) =>
    {
        services.AddMcpCommandServices(serverInfo);
    })
    .UseCommand((commandOptions) =>
    {
        // Register individual commands
        commandOptions.AddCommand<InitializeCommand>();
        commandOptions.AddCommand<ListToolsCommand>();
        commandOptions.AddCommand<CallToolCommand>();
        commandOptions.AddCommand<ListResourcesCommand>();
        commandOptions.AddCommand<ReadResourceCommand>();
        commandOptions.AddCommand<ListPromptsCommand>();
        commandOptions.AddCommand<GetPromptCommand>();
        commandOptions.AddCommand<InitializedCommand>();
    })
    .Build();
```

### Registering Handlers

Handler registration works the same way as before:

```csharp
// Get the handler registry from DI
var handlerRegistry = host.Services.GetRequiredService<IMcpHandlerRegistry>();

// Register handlers
handlerRegistry.RegisterTool("echo", new EchoToolHandler());
handlerRegistry.RegisterResource("file://example", new FileResourceHandler());
handlerRegistry.RegisterPrompt("greeting", new GreetingPromptHandler());
```

## Benefits

1. **Code Reusability**: Same command logic works for both TCP and HTTP
2. **Better Maintainability**: Single place to modify each MCP method's behavior
3. **Extensibility**: Easy to add new MCP methods or transports
4. **SuperSocket Integration**: Leverages SuperSocket's built-in command pipeline
5. **Consistency**: Follows SuperSocket's established patterns and conventions

## Migration from Legacy Implementation

The legacy `McpServer` and `McpHttpServer` classes are still available but marked as obsolete. For new implementations, use the SuperSocket host builder with the `UseMcpCommands` extension.

### Legacy vs New

**Legacy:**
```csharp
var mcpServer = new McpServer(logger, serverInfo);
mcpServer.RegisterTool("echo", new EchoToolHandler());
```

**New:**
```csharp
var host = SuperSocketHostBuilder.Create<McpMessage, McpPipelineFilter>()
    .UseMcpCommands(serverInfo)
    .Build();
    
var handlerRegistry = host.Services.GetRequiredService<IMcpHandlerRegistry>();
handlerRegistry.RegisterTool("echo", new EchoToolHandler());
```