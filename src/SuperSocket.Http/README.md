# SuperSocket HTTP Keep-Alive and Server-Sent Events

This enhancement adds comprehensive support for HTTP Keep-Alive connections and Server-Sent Events (SSE) to SuperSocket.Http.

## Features

### HTTP Keep-Alive Support
- **Connection Reuse**: Multiple HTTP requests over a single connection
- **Automatic Lifecycle Management**: Proper connection handling based on HTTP headers
- **Configurable Timeouts**: Customizable keep-alive timeout settings
- **HTTP/1.1 Compliance**: Full support for HTTP/1.1 keep-alive semantics

### Server-Sent Events (SSE)
- **Real-time Streaming**: Push data to clients in real-time using `text/event-stream`
- **Event Types**: Support for custom event types and JSON events
- **Event IDs**: Automatic event ID generation and tracking for client reconnection
- **Heartbeat Support**: Automatic heartbeat to keep connections alive
- **CORS Support**: Built-in CORS headers for cross-origin requests

## Quick Start

### Basic HTTP Keep-Alive Server

```csharp
using SuperSocket.Http;

var hostBuilder = Host.CreateDefaultBuilder()
    .AsSuperSocketHostBuilder<HttpRequest, HttpKeepAliveFilter>()
    .UsePackageHandler(async (session, request) =>
    {
        // Use extension methods for easy response handling
        await session.SendJsonResponseAsync(
            $"{{\"path\": \"{request.Path}\", \"keepAlive\": {request.KeepAlive}}}"
        );
    })
    .ConfigureSuperSocket(options =>
    {
        options.Name = "HttpKeepAliveServer";
        options.Listeners = new[] { new ListenOptions { Ip = "Any", Port = 8080 } };
    });

await hostBuilder.Build().RunAsync();
```

### Server-Sent Events Stream

```csharp
.UsePackageHandler(async (session, request) =>
{
    if (request.Path == "/events" && request.IsSSERequest())
    {
        // Start SSE stream
        var sseWriter = await session.StartSSEAsync();
        
        // Send events
        await sseWriter.SendEventAsync("Hello SSE!", "greeting");
        await sseWriter.SendJsonEventAsync("{\"type\": \"data\", \"value\": 42}");
        
        // Start automatic heartbeat
        _ = sseWriter.StartHeartbeatAsync(cancellationToken);
        
        // Send more events as needed...
        await sseWriter.SendCloseEventAsync();
    }
    else
    {
        await session.SendHttpResponseAsync(200, "Use /events for SSE", "text/plain");
    }
})
```

## API Reference

### HttpResponse Class

```csharp
var response = new HttpResponse(200, "OK");
response.SetContentType("application/json");
response.Body = "{\"message\": \"Hello World\"}";
response.KeepAlive = true;

// Convert to bytes for sending
byte[] responseBytes = response.ToBytes();
```

### HttpRequest Extensions

```csharp
// Check for keep-alive
bool keepAlive = request.IsKeepAliveRequest();

// Check for SSE support
bool acceptsSSE = request.IsSSERequest();

// Get SSE last event ID for reconnection
string lastEventId = request.LastEventId;
```

### ServerSentEventWriter

```csharp
var sseWriter = new ServerSentEventWriter(connection);
await sseWriter.SendInitialResponseAsync();

// Send different types of events
await sseWriter.SendEventAsync("Simple message");
await sseWriter.SendEventAsync("Custom event", "custom-type", "event-1");
await sseWriter.SendJsonEventAsync("{\"data\": \"json-payload\"}");
await sseWriter.SendHeartbeatAsync();
```

### Session Extensions

```csharp
// Simple responses
await session.SendHttpResponseAsync(200, "OK", "text/plain");
await session.SendJsonResponseAsync("{\"status\": \"success\"}");

// SSE operations
var sseWriter = session.CreateSSEWriter();
var sseWriter = await session.StartSSEAsync(); // Sends initial headers automatically
```

## Configuration Options

### HttpKeepAliveOptions

```csharp
var keepAliveOptions = new HttpKeepAliveOptions
{
    KeepAliveTimeoutSeconds = 60,        // Connection timeout
    MaxRequestsPerConnection = 1000,     // Max requests per connection
    EnableKeepAlive = true               // Enable/disable keep-alive
};
```

### ServerSentEventsOptions

```csharp
var sseOptions = new ServerSentEventsOptions
{
    HeartbeatIntervalSeconds = 30,       // Heartbeat interval
    DefaultRetryIntervalMs = 3000,       // Client retry interval
    EnableCors = true,                   // Enable CORS headers
    CorsOrigin = "*",                    // CORS origin
    CorsAllowedHeaders = "Cache-Control" // CORS headers
};

var sseWriter = new ServerSentEventWriter(connection, sseOptions);
```

## Client-Side JavaScript

### Basic SSE Client

```javascript
const eventSource = new EventSource('/events');

eventSource.onopen = function(event) {
    console.log('SSE connection opened');
};

eventSource.onmessage = function(event) {
    console.log('Message:', event.data, 'ID:', event.lastEventId);
};

eventSource.addEventListener('custom-type', function(event) {
    console.log('Custom event:', event.data);
});

eventSource.onerror = function(event) {
    console.error('SSE error:', event);
};
```

### Keep-Alive HTTP Requests

```javascript
// Modern browsers automatically handle keep-alive for fetch requests
fetch('/api/data')
    .then(response => response.json())
    .then(data => console.log(data));

// Multiple requests will reuse the same connection
for (let i = 0; i < 5; i++) {
    fetch(`/api/data/${i}`)
        .then(response => response.json())
        .then(data => console.log(`Request ${i}:`, data));
}
```

## Examples

See the `/samples` directory for complete examples:

- **BasicHttpKeepAlive**: Simple HTTP server with keep-alive support
- **ServerSentEventsDemo**: Real-time data streaming with SSE
- **HttpApiServer**: REST API server with keep-alive and JSON responses

## Performance Considerations

### Keep-Alive Benefits
- **Reduced Connection Overhead**: Fewer TCP handshakes
- **Lower Latency**: No connection establishment delay for subsequent requests
- **Resource Efficiency**: Fewer server sockets and client connections

### SSE Considerations
- **Memory Usage**: Each SSE connection holds server resources
- **Scaling**: Consider connection limits and load balancing
- **Heartbeat**: Balance between responsiveness and resource usage

## Compatibility

- **HTTP/1.0**: Basic support, keep-alive as extension
- **HTTP/1.1**: Full keep-alive support (default behavior)
- **HTTP/2**: Foundation for future HTTP/2 implementation
- **Browsers**: All modern browsers support SSE and keep-alive

## Migration from Basic HTTP

1. **Replace** `HttpPipelineFilter` with `HttpKeepAliveFilter`
2. **Use** `HttpResponse` class instead of manual response building
3. **Leverage** extension methods for cleaner code
4. **Add** SSE endpoints as needed

```csharp
// Before
.UsePackageHandler(async (s, p) =>
{
    var response = "HTTP/1.1 200 OK\r\n" +
                  "Content-Type: application/json\r\n" +
                  "Content-Length: 26\r\n\r\n" +
                  "{\"message\": \"Hello\"}";
    await s.SendAsync(Encoding.UTF8.GetBytes(response));
})

// After
.UsePackageHandler(async (s, p) =>
{
    await s.SendJsonResponseAsync("{\"message\": \"Hello\"}");
})
```

## Troubleshooting

### Common Issues

1. **Connection Not Reused**: Check that client sends `Connection: keep-alive` header
2. **SSE Not Working**: Verify `Accept: text/event-stream` header
3. **CORS Issues**: Configure SSE options with appropriate CORS settings
4. **Memory Leaks**: Ensure proper disposal of SSE writers and heartbeat tasks

### Debugging

Enable logging to see connection lifecycle:

```csharp
.ConfigureLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
})
```

## Future Enhancements

- **HTTP/2 Support**: Binary framing and multiplexing
- **WebSocket Upgrade**: Seamless protocol switching  
- **Compression**: Gzip/deflate support for responses
- **Caching**: Built-in HTTP caching headers support