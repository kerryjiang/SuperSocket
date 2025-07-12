using System.Text.Json.Serialization;

namespace SuperSocket.MCP.Models;

/// <summary>
/// Server capabilities for MCP protocol
/// </summary>
public class McpServerCapabilities
{
    /// <summary>
    /// Logging capabilities
    /// </summary>
    [JsonPropertyName("logging")]
    public McpLoggingCapabilities? Logging { get; set; }

    /// <summary>
    /// Tools capabilities
    /// </summary>
    [JsonPropertyName("tools")]
    public McpToolsCapabilities? Tools { get; set; }

    /// <summary>
    /// Resources capabilities
    /// </summary>
    [JsonPropertyName("resources")]
    public McpResourcesCapabilities? Resources { get; set; }

    /// <summary>
    /// Prompts capabilities
    /// </summary>
    [JsonPropertyName("prompts")]
    public McpPromptsCapabilities? Prompts { get; set; }
}

/// <summary>
/// Client capabilities for MCP protocol
/// </summary>
public class McpClientCapabilities
{
    /// <summary>
    /// Roots capabilities
    /// </summary>
    [JsonPropertyName("roots")]
    public McpRootsCapabilities? Roots { get; set; }

    /// <summary>
    /// Sampling capabilities
    /// </summary>
    [JsonPropertyName("sampling")]
    public McpSamplingCapabilities? Sampling { get; set; }
}

/// <summary>
/// Logging capabilities
/// </summary>
public class McpLoggingCapabilities
{
    // Empty object indicates logging is supported
}

/// <summary>
/// Tools capabilities
/// </summary>
public class McpToolsCapabilities
{
    /// <summary>
    /// Indicates if list_changed notifications are supported
    /// </summary>
    [JsonPropertyName("listChanged")]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// Resources capabilities
/// </summary>
public class McpResourcesCapabilities
{
    /// <summary>
    /// Indicates if list_changed notifications are supported
    /// </summary>
    [JsonPropertyName("listChanged")]
    public bool? ListChanged { get; set; }

    /// <summary>
    /// Indicates if subscribe/unsubscribe is supported
    /// </summary>
    [JsonPropertyName("subscribe")]
    public bool? Subscribe { get; set; }
}

/// <summary>
/// Prompts capabilities
/// </summary>
public class McpPromptsCapabilities
{
    /// <summary>
    /// Indicates if list_changed notifications are supported
    /// </summary>
    [JsonPropertyName("listChanged")]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// Roots capabilities
/// </summary>
public class McpRootsCapabilities
{
    /// <summary>
    /// Indicates if list_changed notifications are supported
    /// </summary>
    [JsonPropertyName("listChanged")]
    public bool? ListChanged { get; set; }
}

/// <summary>
/// Sampling capabilities
/// </summary>
public class McpSamplingCapabilities
{
    // Empty object indicates sampling is supported
}