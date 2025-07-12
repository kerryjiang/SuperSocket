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
        // Get services
        var logger = session.Server.ServiceProvider.GetService(typeof(ILogger<McpServer>)) as ILogger<McpServer>;
        var serverInfo = new McpServerInfo
        {
            Name = "SuperSocket MCP Server",
            Version = "1.0.0",
            ProtocolVersion = "2024-11-05"
        };
        
        var mcpServer = new McpServer(logger!, serverInfo);
        
        // Register sample tools
        mcpServer.RegisterTool("echo", new EchoToolHandler());
        mcpServer.RegisterTool("math", new MathToolHandler());
        
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