namespace SuperSocket.MCP.Abstractions;

/// <summary>
/// MCP prompt definition
/// </summary>
public class McpPrompt
{
    /// <summary>
    /// Prompt name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Prompt description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Prompt arguments schema
    /// </summary>
    public object? ArgumentsSchema { get; set; }
}