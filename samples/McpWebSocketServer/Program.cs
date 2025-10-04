using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.WebSocket.Server;

namespace McpWebSocketServer
{
    /// <summary>
    /// MCP Server implementation that communicates over WebSocket connections
    /// This uses the SuperSocket.MCP library's command system for proper MCP protocol handling
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting MCP WebSocket Server...");
            Console.WriteLine("This server provides MCP functionality over WebSocket connections.");
            Console.WriteLine("Connect using: ws://localhost:8080 with sub-protocol 'mcp'");
            Console.WriteLine("----------------------------------------");

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
                    services.AddSingleton<IMcpToolHandler, WebInfoToolHandler>();
                })
                .UseMcp("mcp") // Use MCP with "mcp" sub-protocol - this handles all the command routing
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            await host.RunAsync();
        }
    }

    /// <summary>
    /// Simple echo tool that returns the input message
    /// </summary>
    public class EchoToolHandler : IMcpToolHandler
    {
        public Task<McpTool> GetToolDefinitionAsync()
        {
            return Task.FromResult(new McpTool
            {
                Name = "echo",
                Description = "Echo back the input message via WebSocket",
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
            var message = arguments.TryGetValue("message", out var msg) ? msg.ToString() : "Hello WebSocket!";
            return Task.FromResult(new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = $"WebSocket Echo: {message}" }
                }
            });
        }
    }

    /// <summary>
    /// Math tool that performs basic arithmetic operations
    /// </summary>
    public class MathToolHandler : IMcpToolHandler
    {
        public Task<McpTool> GetToolDefinitionAsync()
        {
            return Task.FromResult(new McpTool
            {
                Name = "math",
                Description = "Perform basic math operations (add, subtract, multiply, divide)",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        operation = new { type = "string", description = "The operation to perform", @enum = new[] { "add", "subtract", "multiply", "divide" } },
                        a = new { type = "number", description = "First number" },
                        b = new { type = "number", description = "Second number" }
                    },
                    required = new[] { "operation", "a", "b" }
                }
            });
        }

        public Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            try
            {
                var operation = arguments.TryGetValue("operation", out var op) ? op.ToString() : "";
                var a = Convert.ToDouble(arguments.TryGetValue("a", out var aVal) ? aVal : 0);
                var b = Convert.ToDouble(arguments.TryGetValue("b", out var bVal) ? bVal : 0);

                double result = operation switch
                {
                    "add" => a + b,
                    "subtract" => a - b,
                    "multiply" => a * b,
                    "divide" => b != 0 ? a / b : throw new DivideByZeroException("Cannot divide by zero"),
                    _ => throw new ArgumentException($"Unknown operation: {operation}")
                };

                return Task.FromResult(new McpToolResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent { Type = "text", Text = $"Result: {a} {operation} {b} = {result}" }
                    }
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new McpToolResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent { Type = "text", Text = $"Error: {ex.Message}" }
                    },
                    IsError = true
                });
            }
        }
    }

    /// <summary>
    /// Time tool that provides current server time
    /// </summary>
    public class TimeToolHandler : IMcpToolHandler
    {
        public Task<McpTool> GetToolDefinitionAsync()
        {
            return Task.FromResult(new McpTool
            {
                Name = "time",
                Description = "Get the current server time in various formats",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        format = new { type = "string", description = "Time format", @enum = new[] { "iso", "local", "utc", "unix" } }
                    }
                }
            });
        }

        public Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            var format = arguments.TryGetValue("format", out var fmt) ? fmt.ToString() : "iso";
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            var timeString = format switch
            {
                "iso" => utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                "local" => now.ToString("yyyy-MM-dd HH:mm:ss"),
                "utc" => utcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                "unix" => ((DateTimeOffset)utcNow).ToUnixTimeSeconds().ToString(),
                _ => utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            return Task.FromResult(new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = $"Current time ({format}): {timeString}" }
                }
            });
        }
    }

    /// <summary>
    /// Web information tool that provides details about the WebSocket connection
    /// </summary>
    public class WebInfoToolHandler : IMcpToolHandler
    {
        public Task<McpTool> GetToolDefinitionAsync()
        {
            return Task.FromResult(new McpTool
            {
                Name = "webinfo",
                Description = "Get information about the WebSocket connection and server",
                InputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            });
        }

        public Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            var info = new
            {
                Protocol = "WebSocket",
                SubProtocol = "mcp",
                Server = "SuperSocket MCP WebSocket Server",
                Framework = ".NET 8.0",
                Transport = "WebSocket over TCP",
                Features = new[] { "Binary frames", "Text frames", "Ping/Pong", "Sub-protocols", "Extensions" }
            };

            var infoText = $"""
                WebSocket MCP Server Information:
                - Protocol: {info.Protocol}
                - Sub-Protocol: {info.SubProtocol}
                - Server: {info.Server}
                - Framework: {info.Framework}
                - Transport: {info.Transport}
                - Features: {string.Join(", ", info.Features)}
                """;

            return Task.FromResult(new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = infoText }
                }
            });
        }
    }
}