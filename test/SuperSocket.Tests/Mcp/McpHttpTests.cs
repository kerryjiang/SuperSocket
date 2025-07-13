using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SuperSocket.Http;
using SuperSocket.MCP;
using SuperSocket.MCP.Models;
using SuperSocket.ProtoBase;
using Xunit;

namespace SuperSocket.Tests.Mcp
{
    public class McpHttpTests
    {
        [Fact]
        public void McpHttpRequest_ParsesJsonRpcFromHttpPost()
        {
            // Arrange
            var jsonRpc = new McpMessage
            {
                JsonRpc = "2.0",
                Id = 1,
                Method = "tools/call",
                Params = new { name = "echo", arguments = new { message = "test" } }
            };
            
            var json = JsonSerializer.Serialize(jsonRpc);
            var httpRequest = new HttpRequest("POST", "/mcp", "HTTP/1.1", new System.Collections.Specialized.NameValueCollection());
            httpRequest.Body = json;
            
            // Act
            var mcpHttpRequest = new McpHttpRequest(httpRequest);
            
            // Assert
            Assert.NotNull(mcpHttpRequest.McpMessage);
            Assert.Equal("2.0", mcpHttpRequest.McpMessage.JsonRpc);
            Assert.Equal("1", mcpHttpRequest.McpMessage.Id?.ToString());
            Assert.Equal("tools/call", mcpHttpRequest.McpMessage.Method);
            Assert.True(mcpHttpRequest.McpMessage.IsRequest);
        }
        
        [Fact]
        public void McpHttpRequest_HandlesGetRequestWithoutBody()
        {
            // Arrange
            var httpRequest = new HttpRequest("GET", "/mcp/capabilities", "HTTP/1.1", new System.Collections.Specialized.NameValueCollection());
            
            // Act
            var mcpHttpRequest = new McpHttpRequest(httpRequest);
            
            // Assert
            Assert.Null(mcpHttpRequest.McpMessage);
            Assert.Equal("GET", mcpHttpRequest.HttpRequest.Method);
            Assert.Equal("/mcp/capabilities", mcpHttpRequest.HttpRequest.Path);
        }
        
        [Fact]
        public void McpHttpRequest_ThrowsOnInvalidJson()
        {
            // Arrange
            var httpRequest = new HttpRequest("POST", "/mcp", "HTTP/1.1", new System.Collections.Specialized.NameValueCollection());
            httpRequest.Body = "invalid json";
            
            // Act & Assert
            Assert.Throws<ProtocolException>(() => new McpHttpRequest(httpRequest));
        }
        
        [Fact]
        public void McpMessage_DetectsRequestResponseNotification()
        {
            // Arrange & Act
            var request = new McpMessage { JsonRpc = "2.0", Id = 1, Method = "test" };
            var response = new McpMessage { JsonRpc = "2.0", Id = 1, Result = "success" };
            var notification = new McpMessage { JsonRpc = "2.0", Method = "notify" };
            
            // Assert
            Assert.True(request.IsRequest);
            Assert.False(request.IsResponse);
            Assert.False(request.IsNotification);
            
            Assert.False(response.IsRequest);
            Assert.True(response.IsResponse);
            Assert.False(response.IsNotification);
            
            Assert.False(notification.IsRequest);
            Assert.False(notification.IsResponse);
            Assert.True(notification.IsNotification);
        }
    }
}