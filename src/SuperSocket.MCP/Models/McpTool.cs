using SuperSocket.MCP.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperSocket.MCP.Models;

/// <summary>
/// MCP tool definition
/// </summary>
public class McpTool
{
    /// <summary>
    /// Tool name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tool description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// JSON Schema for tool input parameters
    /// </summary>
    [JsonPropertyName("inputSchema")]
    public object? InputSchema { get; set; }
}

/// <summary>
/// Parameters for tools/list request
/// </summary>
public class McpListToolsParams
{
    /// <summary>
    /// Optional cursor for pagination
    /// </summary>
    [JsonPropertyName("cursor")]
    public string? Cursor { get; set; }
}

/// <summary>
/// Result for tools/list request
/// </summary>
public class McpListToolsResult
{
    /// <summary>
    /// List of available tools
    /// </summary>
    [JsonPropertyName("tools")]
    public List<McpTool> Tools { get; set; } = new();

    /// <summary>
    /// Cursor for next page (if applicable)
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// Parameters for tools/call request
/// </summary>
public class McpCallToolParams
{
    /// <summary>
    /// Name of the tool to call
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Arguments for the tool call
    /// </summary>
    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }
}

/// <summary>
/// Result for tools/call request
/// </summary>
public class McpCallToolResult
{
    /// <summary>
    /// Content returned by the tool
    /// </summary>
    [JsonPropertyName("content")]
    public List<McpContent> Content { get; set; } = new();

    /// <summary>
    /// Indicates if the tool call was successful
    /// </summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; set; }
}

/// <summary>
/// Content object for tool results
/// </summary>
public class McpContent
{
    /// <summary>
    /// Content type (e.g., "text", "image")
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Text content (when type is "text")
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Image data (when type is "image")
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>
    /// MIME type for binary content
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}

/// <summary>
/// Tool result for handler interface
/// </summary>
public class McpToolResult
{
    /// <summary>
    /// Content returned by the tool
    /// </summary>
    public List<McpContent> Content { get; set; } = new();

    /// <summary>
    /// Indicates if the tool call was an error
    /// </summary>
    public bool IsError { get; set; }
}

/// <summary>
/// Response for tools/list request
/// </summary>
public class McpToolsListResponse
{
    /// <summary>
    /// List of available tools
    /// </summary>
    [JsonPropertyName("tools")]
    public List<McpTool> Tools { get; set; } = new();

    /// <summary>
    /// Cursor for next page (if applicable)
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// Request for tools/call
/// </summary>
public class McpToolsCallRequest
{
    /// <summary>
    /// Name of the tool to call
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Arguments for the tool call
    /// </summary>
    [JsonPropertyName("arguments")]
    public Dictionary<string, object> Arguments { get; set; } = new();
}

/// <summary>
/// Response for resources/list request
/// </summary>
public class McpResourcesListResponse
{
    /// <summary>
    /// List of available resources
    /// </summary>
    [JsonPropertyName("resources")]
    public List<McpResource> Resources { get; set; } = new();

    /// <summary>
    /// Cursor for next page (if applicable)
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// Request for resources/read
/// </summary>
public class McpResourcesReadRequest
{
    /// <summary>
    /// URI of the resource to read
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Response for prompts/list request
/// </summary>
public class McpPromptsListResponse
{
    /// <summary>
    /// List of available prompts
    /// </summary>
    [JsonPropertyName("prompts")]
    public List<McpPrompt> Prompts { get; set; } = new();

    /// <summary>
    /// Cursor for next page (if applicable)
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

/// <summary>
/// Request for prompts/get
/// </summary>
public class McpPromptsGetRequest
{
    /// <summary>
    /// Name of the prompt to get
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Arguments for the prompt
    /// </summary>
    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }
}