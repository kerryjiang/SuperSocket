# MCP Stdio Server Sample

This sample demonstrates how to create an MCP (Model Context Protocol) server that communicates over stdio (stdin/stdout) using SuperSocket's new console connection support.

## Overview

MCP servers are typically designed to run as subprocesses and communicate via stdio rather than TCP. This is the standard deployment pattern for MCP servers, where:

- MCP clients spawn the server as a subprocess
- Communication happens via stdin/stdout using newline-delimited JSON-RPC messages
- The server process is managed by the client

## Key Features

- **Stdio Communication**: Uses SuperSocket's new `UseConsole()` extension for stdio-based communication
- **Line-based Protocol**: Updated `McpPipelineFilter` that processes newline-delimited JSON messages
- **Command Architecture**: Uses SuperSocket's command system for proper MCP message handling
- **Tool Implementation**: Includes sample tools (echo, math, file reading)

## Protocol Changes

### Before (TCP/HTTP style):
```
Content-Length: 123
Content-Type: application/json

{"jsonrpc": "2.0", "method": "initialize", ...}
```

### After (Stdio style):
```
{"jsonrpc": "2.0", "method": "initialize", ...}
{"jsonrpc": "2.0", "method": "initialized"}
```

## Available Tools

1. **Echo Tool** - Simple echo functionality
   ```json
   {"method": "tools/call", "params": {"name": "echo", "arguments": {"message": "Hello!"}}}
   ```

2. **Math Tool** - Basic arithmetic operations
   ```json
   {"method": "tools/call", "params": {"name": "math", "arguments": {"operation": "add", "a": 5, "b": 3}}}
   ```

3. **File Read Tool** - Read local text files
   ```json
   {"method": "tools/call", "params": {"name": "read_file", "arguments": {"path": "/path/to/file.txt"}}}
   ```

## Building and Running

```bash
cd samples/McpStdioServer
dotnet build
dotnet run
```

## Usage as MCP Server

### Direct Testing
You can test the server directly by piping JSON messages:

```bash
echo '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test", "version": "1.0"}}}' | dotnet run
```

### Integration with MCP Clients

Configure your MCP client to spawn this server:

```json
{
  "mcpServers": {
    "supersocket-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/McpStdioServer"],
      "cwd": "/path/to/McpStdioServer"
    }
  }
}
```

## MCP Protocol Flow

1. **Initialize**: Client sends initialization message
2. **Initialized**: Server confirms initialization
3. **Tool Discovery**: Client can query available tools via `tools/list`
4. **Tool Execution**: Client calls tools via `tools/call`
5. **Resource Access**: Server can provide resources via `resources/list` and `resources/read`

## Implementation Details

### Console Connection Integration
- Uses SuperSocket's `UseConsole()` extension
- Leverages the new `ConsoleConnection` for stdio communication
- Maintains full SuperSocket feature compatibility

### Pipeline Filter Optimization
- Simplified from Content-Length headers to line-based JSON
- Better suited for stdio communication patterns
- Handles empty lines and malformed input gracefully

### Command-based Architecture
- Uses SuperSocket's command system instead of legacy message handling
- Proper separation of concerns
- Extensible for additional MCP methods

## Logging Considerations

The server minimizes logging to stdout to avoid interfering with the MCP protocol. Important considerations:

- Logs warning level and above to stderr
- Normal MCP communication happens on stdout
- Debug information should be logged to files if needed

## Error Handling

The server includes comprehensive error handling for:
- Malformed JSON messages
- Invalid tool parameters
- File system access errors
- Network/stdio communication issues

All errors are returned as proper JSON-RPC error responses according to the MCP specification.