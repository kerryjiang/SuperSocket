namespace SuperSocket.MCP.Abstractions;

/// <summary>
/// MCP resource content
/// </summary>
public class McpResourceContent
{
    /// <summary>
    /// Resource URI
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the content
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Content data
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Binary data (base64 encoded)
    /// </summary>
    public string? Blob { get; set; }
}