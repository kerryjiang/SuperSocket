using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using SuperSocket.MCP.Models;
using SuperSocket.ProtoBase;

namespace SuperSocket.MCP;

/// <summary>
/// Pipeline filter for MCP protocol over stdio using line-based JSON-RPC format
/// Optimized for console/stdio communication where each message is a complete JSON line
/// </summary>
public class McpPipelineFilter : TerminatorPipelineFilter<McpMessage>
{
    /// <summary>
    /// Initializes a new instance of McpPipelineFilter with line terminator for stdio
    /// </summary>
    public McpPipelineFilter() : base(Encoding.UTF8.GetBytes("\n"))
    {
        // MCP over stdio uses newline-delimited JSON messages
    }

    /// <summary>
    /// Decodes a complete JSON line into an MCP message
    /// </summary>
    /// <param name="buffer">The buffer containing the JSON line</param>
    /// <returns>Decoded MCP message</returns>
    protected override McpMessage DecodePackage(ref ReadOnlySequence<byte> buffer)
    {
        try
        {
            // Convert buffer to string
            var jsonString = buffer.GetString(Encoding.UTF8).Trim();
            
            // Skip empty lines (common in stdio communication)
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null!;
            }

            // Parse JSON-RPC message
            var message = JsonSerializer.Deserialize<McpMessage>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            return message ?? throw new InvalidOperationException("Failed to deserialize MCP message");
        }
        catch (JsonException ex)
        {
            throw new ProtocolException($"Failed to parse MCP JSON message: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new ProtocolException($"Unexpected error parsing MCP message: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Factory for creating MCP pipeline filters
/// </summary>
public class McpPipelineFilterFactory : PipelineFilterFactoryBase<McpMessage>
{
    protected override IPipelineFilter<McpMessage> Create()
    {
        return new McpPipelineFilter();
    }
}