using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Models;
using Xunit;

namespace SuperSocket.Tests.Mcp;

public class McpMessageTests
{
    [Fact]
    public void McpMessage_Should_Detect_Request_Message()
    {
        var message = new McpMessage
        {
            Id = 1,
            Method = "initialize",
            Params = new { }
        };

        Assert.True(message.IsRequest);
        Assert.False(message.IsResponse);
        Assert.False(message.IsNotification);
    }

    [Fact]
    public void McpMessage_Should_Detect_Response_Message()
    {
        var message = new McpMessage
        {
            Id = 1,
            Result = new { }
        };

        Assert.False(message.IsRequest);
        Assert.True(message.IsResponse);
        Assert.False(message.IsNotification);
    }

    [Fact]
    public void McpMessage_Should_Detect_Notification_Message()
    {
        var message = new McpMessage
        {
            Method = "initialized",
            Params = new { }
        };

        Assert.False(message.IsRequest);
        Assert.False(message.IsResponse);
        Assert.True(message.IsNotification);
    }

    [Fact]
    public void McpMessage_Should_Serialize_To_Json()
    {
        var message = new McpMessage
        {
            Id = 1,
            Method = "tools/list",
            Params = new { }
        };

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.Contains("\"jsonrpc\":\"2.0\"", json);
        Assert.Contains("\"method\":\"tools/list\"", json);
        Assert.Contains("\"id\":1", json);
    }

    [Fact]
    public void McpMessage_Should_Deserialize_From_Json()
    {
        var json = "{\"jsonrpc\":\"2.0\",\"method\":\"tools/list\",\"params\":{},\"id\":1}";
        
        var message = JsonSerializer.Deserialize<McpMessage>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(message);
        Assert.Equal("2.0", message.JsonRpc);
        Assert.Equal("tools/list", message.Method);
        Assert.Equal(1, ((JsonElement)message.Id).GetInt32());
    }
}

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
                    message = new { type = "string", description = "Message to echo" }
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

public class McpToolTests
{
    [Fact]
    public async Task EchoTool_Should_Return_Definition()
    {
        var handler = new EchoToolHandler();
        var tool = await handler.GetToolDefinitionAsync();

        Assert.Equal("echo", tool.Name);
        Assert.Equal("Echo back the input message", tool.Description);
        Assert.NotNull(tool.InputSchema);
    }

    [Fact]
    public async Task EchoTool_Should_Execute_Successfully()
    {
        var handler = new EchoToolHandler();
        var args = new Dictionary<string, object>
        {
            { "message", "Hello World" }
        };

        var result = await handler.ExecuteAsync(args);

        Assert.NotNull(result);
        Assert.Single(result.Content);
        Assert.Equal("text", result.Content[0].Type);
        Assert.Equal("Echo: Hello World", result.Content[0].Text);
    }
}