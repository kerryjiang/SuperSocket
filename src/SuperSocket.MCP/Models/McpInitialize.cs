using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperSocket.MCP.Models;

/// <summary>
/// Server information for MCP initialization
/// </summary>
public class McpServerInfo
{
    /// <summary>
    /// Server name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Server version
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Protocol version this server supports
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";
}

/// <summary>
/// Client information for MCP initialization
/// </summary>
public class McpClientInfo
{
    /// <summary>
    /// Client name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Client version
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Initialize request parameters
/// </summary>
public class McpInitializeParams
{
    /// <summary>
    /// Protocol version supported by the client
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    /// <summary>
    /// Client capabilities
    /// </summary>
    [JsonPropertyName("capabilities")]
    public McpClientCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Client information
    /// </summary>
    [JsonPropertyName("clientInfo")]
    public McpClientInfo ClientInfo { get; set; } = new();
}

/// <summary>
/// Initialize response result
/// </summary>
public class McpInitializeResult
{
    /// <summary>
    /// Protocol version supported by the server
    /// </summary>
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    /// <summary>
    /// Server capabilities
    /// </summary>
    [JsonPropertyName("capabilities")]
    public McpServerCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Server information
    /// </summary>
    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; set; } = new();
}

/// <summary>
/// Initialize notification parameters
/// </summary>
public class McpInitializedParams
{
    // Empty object for initialized notification
}