using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Http;
using SuperSocket.MCP.Models;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.MCP.Extensions;

/// <summary>
/// Extension methods for MCP message handling
/// </summary>
public static class McpExtensions
{
    /// <summary>
    /// Sends an MCP message to the session
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="message">MCP message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpMessageAsync(this IAppSession session, McpMessage message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = Encoding.UTF8.GetBytes(json);
        var header = $"Content-Length: {content.Length}\r\n\r\n";
        var headerBytes = Encoding.UTF8.GetBytes(header);

        var fullMessage = new byte[headerBytes.Length + content.Length];
        Array.Copy(headerBytes, 0, fullMessage, 0, headerBytes.Length);
        Array.Copy(content, 0, fullMessage, headerBytes.Length, content.Length);

        await session.SendAsync(fullMessage, cancellationToken);
    }

    /// <summary>
    /// Sends an MCP response message
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="id">Request ID</param>
    /// <param name="result">Response result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpResponseAsync(this IAppSession session, object? id, object result, CancellationToken cancellationToken = default)
    {
        var message = new McpMessage
        {
            Id = id,
            Result = result
        };

        await session.SendMcpMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends an MCP error response
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="id">Request ID</param>
    /// <param name="code">Error code</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="data">Additional error data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpErrorAsync(this IAppSession session, object? id, int code, string errorMessage, object? data = null, CancellationToken cancellationToken = default)
    {
        var message = new McpMessage
        {
            Id = id,
            Error = new McpError
            {
                Code = code,
                Message = errorMessage,
                Data = data
            }
        };

        await session.SendMcpMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends an MCP notification
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="method">Notification method</param>
    /// <param name="parameters">Notification parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpNotificationAsync(this IAppSession session, string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var message = new McpMessage
        {
            Method = method,
            Params = parameters
        };

        await session.SendMcpMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends an MCP request
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="id">Request ID</param>
    /// <param name="method">Request method</param>
    /// <param name="parameters">Request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpRequestAsync(this IAppSession session, object id, string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var message = new McpMessage
        {
            Id = id,
            Method = method,
            Params = parameters
        };

        await session.SendMcpMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Creates an MCP error response message
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <param name="code">Error code</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="data">Additional error data</param>
    /// <returns>MCP error message</returns>
    public static McpMessage CreateMcpError(object? id, int code, string errorMessage, object? data = null)
    {
        return new McpMessage
        {
            Id = id,
            Error = new McpError
            {
                Code = code,
                Message = errorMessage,
                Data = data
            }
        };
    }

    /// <summary>
    /// Creates an MCP response message
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <param name="result">Response result</param>
    /// <returns>MCP response message</returns>
    public static McpMessage CreateMcpResponse(object? id, object result)
    {
        return new McpMessage
        {
            Id = id,
            Result = result
        };
    }

    /// <summary>
    /// Creates an MCP notification message
    /// </summary>
    /// <param name="method">Notification method</param>
    /// <param name="parameters">Notification parameters</param>
    /// <returns>MCP notification message</returns>
    public static McpMessage CreateMcpNotification(string method, object? parameters = null)
    {
        return new McpMessage
        {
            Method = method,
            Params = parameters
        };
    }

    /// <summary>
    /// Creates an MCP request message
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <param name="method">Request method</param>
    /// <param name="parameters">Request parameters</param>
    /// <returns>MCP request message</returns>
    public static McpMessage CreateMcpRequest(object id, string method, object? parameters = null)
    {
        return new McpMessage
        {
            Id = id,
            Method = method,
            Params = parameters
        };
    }

    /// <summary>
    /// Sends an HTTP response with MCP message content
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="message">MCP message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendHttpMcpResponseAsync(this IAppSession session, McpMessage message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var response = new HttpResponse();
        response.SetContentType("application/json");
        response.Body = json;
        
        await session.SendAsync(HttpResponseEncoder.Instance, response, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP error response with MCP error
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="id">Request ID</param>
    /// <param name="code">Error code</param>
    /// <param name="errorMessage">Error message</param>
    /// <param name="data">Additional error data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendHttpMcpErrorAsync(this IAppSession session, object? id, int code, string errorMessage, object? data = null, CancellationToken cancellationToken = default)
    {
        var message = CreateMcpError(id, code, errorMessage, data);
        await session.SendHttpMcpResponseAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP response with MCP result
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="id">Request ID</param>
    /// <param name="result">Response result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendHttpMcpResultAsync(this IAppSession session, object? id, object result, CancellationToken cancellationToken = default)
    {
        var message = CreateMcpResponse(id, result);
        await session.SendHttpMcpResponseAsync(message, cancellationToken);
    }

    /// <summary>
    /// Starts Server-Sent Events connection for MCP notifications
    /// </summary>
    /// <param name="session">Target session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ServerSentEventWriter for sending events</returns>
    public static async Task<ServerSentEventWriter> StartMcpServerSentEventsAsync(this IAppSession session, CancellationToken cancellationToken = default)
    {
        var writer = new ServerSentEventWriter(session.Connection);
        await writer.SendInitialResponseAsync(cancellationToken);
        return writer;
    }

    /// <summary>
    /// Sends an MCP message via Server-Sent Events
    /// </summary>
    /// <param name="writer">SSE writer</param>
    /// <param name="message">MCP message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpEventAsync(this ServerSentEventWriter writer, McpMessage message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var eventType = message.IsRequest ? "request" : message.IsResponse ? "response" : "notification";
        await writer.SendEventAsync(json, eventType, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends an MCP notification via Server-Sent Events
    /// </summary>
    /// <param name="writer">SSE writer</param>
    /// <param name="method">Notification method</param>
    /// <param name="parameters">Notification parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public static async Task SendMcpNotificationEventAsync(this ServerSentEventWriter writer, string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var message = CreateMcpNotification(method, parameters);
        await writer.SendMcpEventAsync(message, cancellationToken);
    }
}