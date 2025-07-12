# SuperSocket MCP HTTP Support

SuperSocket MCP now supports HTTP transport in addition to the original TCP transport with Content-Length headers. This enables easier integration with web applications and provides Server-Sent Events (SSE) support for real-time notifications.

## Features

- **HTTP POST**: Send MCP JSON-RPC requests via HTTP POST to `/mcp`
- **Server-Sent Events**: Connect to `/mcp/events` for real-time notifications
- **Capabilities Endpoint**: GET `/mcp/capabilities` to discover server capabilities
- **Backward Compatibility**: Original TCP transport still works unchanged

## Usage

### HTTP POST Requests

Send MCP JSON-RPC requests to the `/mcp` endpoint:

```bash
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
```

### Server-Sent Events

Connect to the SSE endpoint for real-time notifications:

```javascript
const eventSource = new EventSource('http://localhost:8080/mcp/events');

eventSource.onmessage = function(event) {
  const data = JSON.parse(event.data);
  console.log('Received:', data);
};

eventSource.addEventListener('notification', function(event) {
  const notification = JSON.parse(event.data);
  console.log('Notification:', notification);
});
```

### Capabilities

Check server capabilities:

```bash
curl http://localhost:8080/mcp/capabilities
```

## Example Server

```csharp
using SuperSocket.MCP;
using SuperSocket.Server.Host;

var host = SuperSocketHostBuilder.Create<McpHttpRequest, McpHttpPipelineFilter>(args)
    .UsePackageHandler(async (session, request) =>
    {
        var mcpServer = new McpHttpServer(logger, serverInfo);
        
        // Register tools
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

## API Endpoints

### POST /mcp
- **Purpose**: Handle MCP JSON-RPC requests
- **Content-Type**: application/json
- **Body**: MCP JSON-RPC message
- **Response**: JSON-RPC response

### GET /mcp/events
- **Purpose**: Server-Sent Events stream
- **Accept**: text/event-stream
- **Response**: SSE stream with MCP notifications

### GET /mcp/capabilities
- **Purpose**: Server capability discovery
- **Response**: JSON object with server capabilities

## Benefits

1. **Web Integration**: Easy integration with web applications
2. **Real-time Updates**: SSE support for live notifications
3. **Standard HTTP**: Uses familiar HTTP protocols
4. **Backward Compatible**: Original TCP transport unchanged
5. **RESTful**: Standard HTTP methods and status codes

## Transport Comparison

| Feature | TCP (Original) | HTTP |
|---------|----------------|------|
| Protocol | Content-Length + JSON | HTTP + JSON |
| Port | 3000 (default) | 8080 (default) |
| Streaming | Bidirectional | SSE for notifications |
| Web Compatible | No | Yes |
| Firewall Friendly | No | Yes |
| Load Balancer Support | Limited | Full |

Both transports support the same MCP features and can be used simultaneously.