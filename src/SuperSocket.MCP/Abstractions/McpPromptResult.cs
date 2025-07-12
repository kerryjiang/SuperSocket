using System.Collections.Generic;

namespace SuperSocket.MCP.Abstractions;

/// <summary>
/// MCP prompt result
/// </summary>
public class McpPromptResult
{
    /// <summary>
    /// Prompt description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Prompt messages
    /// </summary>
    public List<McpPromptMessage> Messages { get; set; } = new();
}