using SuperSocket.MCP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.MCP.Abstractions;

/// <summary>
/// Interface for implementing MCP tools
/// </summary>
public interface IMcpToolHandler
{
    /// <summary>
    /// Gets the tool definition
    /// </summary>
    /// <returns>Tool definition</returns>
    Task<McpTool> GetToolDefinitionAsync();

    /// <summary>
    /// Executes the tool with the provided arguments
    /// </summary>
    /// <param name="arguments">Tool arguments</param>
    /// <returns>Tool execution result</returns>
    Task<McpToolResult> ExecuteAsync(Dictionary<string, object> arguments);
}