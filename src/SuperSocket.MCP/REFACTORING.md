# MCP Command Refactoring

This document outlines the refactoring performed to improve the SuperSocket MCP implementation using the command pattern.

## Overview

The MCP (Model Context Protocol) implementation has been refactored to use SuperSocket's command pattern, reducing code duplication and improving maintainability.

## Key Changes

### 1. Shared Handler Registry

- **Before**: Each server (`McpServer` and `McpHttpServer`) maintained its own handler collections
- **After**: Shared `IMcpHandlerRegistry` and `McpHandlerRegistry` implementation
- **Benefit**: Single registration point for all handlers, reducing duplication

### 2. Command-Based Architecture

- **Before**: Switch-based message handling in server classes
- **After**: Individual command classes for each MCP method
- **Benefit**: Each MCP method has its own focused command class

### 3. Command Classes Created

- `InitializeCommand` - Handles MCP initialization
- `ListToolsCommand` - Handles tools/list requests
- `CallToolCommand` - Handles tools/call requests
- `ListResourcesCommand` - Handles resources/list requests
- `ReadResourceCommand` - Handles resources/read requests
- `ListPromptsCommand` - Handles prompts/list requests
- `GetPromptCommand` - Handles prompts/get requests

### 4. Base Command Infrastructure

- `McpCommandBase` - Base class for all MCP commands
- Provides error handling, response creation, and session management
- Integrates with SuperSocket's `IAsyncCommand<T>` interface

### 5. Updated Server Implementations

- Both `McpServer` and `McpHttpServer` now use the shared handler registry
- Reduced code duplication between TCP and HTTP implementations
- Maintained backward compatibility

## Benefits

1. **Code Reusability**: Same command logic works for both TCP and HTTP
2. **Better Maintainability**: Single place to modify each MCP method's behavior
3. **Extensibility**: Easy to add new MCP methods or transports
4. **SuperSocket Integration**: Leverages SuperSocket's built-in command pipeline
5. **Consistency**: Follows SuperSocket's established patterns

## Usage Examples

### Basic Handler Registration

```csharp
// Create handler registry
var handlerRegistry = new McpHandlerRegistry(logger);

// Register handlers once - usable by both TCP and HTTP
handlerRegistry.RegisterTool("echo", new EchoToolHandler());
handlerRegistry.RegisterResource("file://example", new FileResourceHandler());
handlerRegistry.RegisterPrompt("greeting", new GreetingPromptHandler());
```

### Command Execution

```csharp
// Create and execute a command
var command = new InitializeCommand(logger, handlerRegistry, serverInfo);
await command.ExecuteAsync(session, message, cancellationToken);
```

### Service Registration

```csharp
// Register MCP command services
services.AddMcpCommandServices(serverInfo);
```

## Migration Guide

### For Existing Users

The refactoring maintains backward compatibility:

1. Existing `McpServer` and `McpHttpServer` constructors work as before
2. Handler registration methods remain unchanged
3. The same pipeline filters are used

### For New Implementations

New implementations can take advantage of:

1. Shared handler registry for consistent behavior
2. Individual command classes for better organization
3. Service registration helpers for DI integration

## Files Changed

- `McpServer.cs` - Updated to use shared handler registry
- `McpHttpServer.cs` - Updated to use shared handler registry
- `IMcpHandlerRegistry.cs` - New shared registry interface
- `McpHandlerRegistry.cs` - Shared registry implementation
- `Commands/` - New directory with command implementations
- `Extensions/McpCommandServiceExtensions.cs` - Service registration helpers
- `Examples/McpCommandExample.cs` - Usage demonstration

## Testing

The refactored system can be tested using the provided example:

```csharp
await McpCommandExample.RunExampleAsync();
```

This demonstrates:
- Handler registration
- Command execution
- Response handling
- Benefits of the new architecture