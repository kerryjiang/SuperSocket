using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.MCP.Models;

namespace SuperSocket.MCP.Abstractions;

/// <summary>
/// MCP prompt message
/// </summary>
public class McpPromptMessage
{
    /// <summary>
    /// Message role
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Message content
    /// </summary>
    public List<McpContent> Content { get; set; } = new();
}