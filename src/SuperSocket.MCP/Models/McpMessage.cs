using System.Text.Json.Serialization;
using SuperSocket.ProtoBase;

namespace SuperSocket.MCP.Models;

/// <summary>
/// Represents the base JSON-RPC 2.0 message structure for MCP protocol
/// </summary>
public class McpMessage : IKeyedPackageInfo<string>
{
    /// <summary>
    /// JSON-RPC version (must be "2.0")
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    /// <summary>
    /// Method name for requests
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>
    /// Parameters for method calls
    /// </summary>
    [JsonPropertyName("params")]
    public object? Params { get; set; }

    /// <summary>
    /// Request ID for tracking requests and responses
    /// </summary>
    [JsonPropertyName("id")]
    public object? Id { get; set; }

    /// <summary>
    /// Result for successful responses
    /// </summary>
    [JsonPropertyName("result")]
    public object? Result { get; set; }

    /// <summary>
    /// Error information for error responses
    /// </summary>
    [JsonPropertyName("error")]
    public McpError? Error { get; set; }

    /// <summary>
    /// Determines if this is a request message
    /// </summary>
    [JsonIgnore]
    public bool IsRequest => !string.IsNullOrEmpty(Method) && Id != null;

    /// <summary>
    /// Determines if this is a response message
    /// </summary>
    [JsonIgnore]
    public bool IsResponse => Id != null && (Result != null || Error != null);

    /// <summary>
    /// Determines if this is a notification message
    /// </summary>
    [JsonIgnore]
    public bool IsNotification => !string.IsNullOrEmpty(Method) && Id == null;

    /// <summary>
    /// Gets the key for SuperSocket's command dispatch system
    /// </summary>
    [JsonIgnore]
    public string Key => Method ?? string.Empty;
}

/// <summary>
/// Represents a JSON-RPC 2.0 error object
/// </summary>
public class McpError
{
    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error data
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

/// <summary>
/// Standard JSON-RPC 2.0 error codes
/// </summary>
public static class McpErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
    public const int ServerError = -32000;
}