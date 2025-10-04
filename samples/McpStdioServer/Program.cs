using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Commands;
using SuperSocket.MCP.Extensions;
using SuperSocket.MCP.Models;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Host;
using SuperSocket.Command;

namespace McpStdioServer
{
    /// <summary>
    /// MCP Server implementation that communicates over stdio (stdin/stdout)
    /// This is the standard way MCP servers are used - they communicate via stdio
    /// and are typically spawned by MCP clients as subprocess
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Disable console buffering for immediate I/O
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

            var host = SuperSocketHostBuilder.Create<McpMessage, McpPipelineFilter>()
                .UseConsole() // Use stdio instead of TCP - this is the key change!
                .UseCommand<string, McpMessage>(commandOptions =>
                {
                    // Register MCP commands
                    commandOptions.AddCommand<InitializeCommand>();
                    commandOptions.AddCommand<InitializedCommand>();
                    commandOptions.AddCommand<ListToolsCommand>();
                    commandOptions.AddCommand<CallToolCommand>();
                    commandOptions.AddCommand<ListResourcesCommand>();
                    commandOptions.AddCommand<ListPromptsCommand>();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    // Register MCP services
                    services.AddSingleton<IMcpHandlerRegistry, McpHandlerRegistry>();
                    services.AddSingleton<McpServerInfo>(new McpServerInfo
                    {
                        Name = "SuperSocket MCP Stdio Server",
                        Version = "1.0.0",
                        ProtocolVersion = "2024-11-05"
                    });
                    
                    // Register sample tools
                    services.AddSingleton<IMcpToolHandler, EchoToolHandler>();
                    services.AddSingleton<IMcpToolHandler, MathToolHandler>();
                    services.AddSingleton<IMcpToolHandler, FileReadToolHandler>();
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    // Minimal logging to stderr to avoid interfering with MCP protocol on stdout
                    loggingBuilder.AddConsole(options =>
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Warning;
                    });
                    loggingBuilder.SetMinimumLevel(LogLevel.Warning);
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
    /// File reading tool for accessing local files
    /// </summary>
    public class FileReadToolHandler : IMcpToolHandler
    {
        public Task<McpTool> GetToolDefinitionAsync()
        {
            return Task.FromResult(new McpTool
            {
                Name = "read_file",
                Description = "Read the contents of a text file",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        path = new { type = "string", description = "Path to the file to read" }
                    },
                    required = new[] { "path" }
                }
            });
        }

        public async Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            try
            {
                var path = arguments.TryGetValue("path", out var p) ? p.ToString() : "";
                
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("File path is required");
                }

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"File not found: {path}");
                }

                var content = await File.ReadAllTextAsync(path);
                
                return new McpToolResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent { Type = "text", Text = content }
                    }
                };
            }
            catch (Exception ex)
            {
                return new McpToolResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent { Type = "text", Text = $"Error reading file: {ex.Message}" }
                    },
                    IsError = true
                };
            }
        }
    }
}