namespace SuperSocket.MCP.Abstractions;

/// <summary>
/// MCP resource definition
/// </summary>
public class McpResource
{
    /// <summary>
    /// Resource URI
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Resource name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Resource description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// MIME type of the resource
    /// </summary>
    public string? MimeType { get; set; }
}