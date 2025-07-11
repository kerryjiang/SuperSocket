using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.MCP.Models;

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

/// <summary>
/// Interface for implementing MCP resources
/// </summary>
public interface IMcpResourceHandler
{
    /// <summary>
    /// Gets the resource definition
    /// </summary>
    /// <returns>Resource definition</returns>
    Task<McpResource> GetResourceDefinitionAsync();

    /// <summary>
    /// Reads the resource content
    /// </summary>
    /// <param name="uri">Resource URI</param>
    /// <returns>Resource content</returns>
    Task<McpResourceContent> ReadAsync(string uri);
}

/// <summary>
/// Interface for implementing MCP prompts
/// </summary>
public interface IMcpPromptHandler
{
    /// <summary>
    /// Gets the prompt definition
    /// </summary>
    /// <returns>Prompt definition</returns>
    Task<McpPrompt> GetPromptDefinitionAsync();

    /// <summary>
    /// Gets the prompt with the provided arguments
    /// </summary>
    /// <param name="arguments">Prompt arguments</param>
    /// <returns>Prompt result</returns>
    Task<McpPromptResult> GetPromptAsync(Dictionary<string, object>? arguments = null);
}

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