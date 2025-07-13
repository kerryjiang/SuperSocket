using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using SuperSocket.Http;
using SuperSocket.MCP.Models;
using SuperSocket.ProtoBase;

namespace SuperSocket.MCP;

/// <summary>
/// Pipeline filter for MCP protocol over HTTP
/// </summary>
public class McpHttpPipelineFilter : IPipelineFilter<McpHttpRequest>
{
    private readonly HttpPipelineFilter _httpFilter;

    public McpHttpPipelineFilter()
    {
        _httpFilter = new HttpPipelineFilter();
    }

    public IPackageDecoder<McpHttpRequest> Decoder { get; set; }

    public IPipelineFilter<McpHttpRequest> NextFilter { get; set; }

    public object Context { get; set; }

    public McpHttpRequest Filter(ref SequenceReader<byte> reader)
    {
        var httpRequest = _httpFilter.Filter(ref reader);
        if (httpRequest == null)
            return null;

        return new McpHttpRequest(httpRequest);
    }

    public void Reset()
    {
        _httpFilter.Reset();
    }
}

/// <summary>
/// Represents an HTTP request containing MCP data
/// </summary>
public class McpHttpRequest
{
    public HttpRequest HttpRequest { get; }
    public McpMessage McpMessage { get; }

    public McpHttpRequest(HttpRequest httpRequest)
    {
        HttpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
        
        // Parse MCP message from HTTP body for POST requests
        if (string.Equals(httpRequest.Method, "POST", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(httpRequest.Body))
        {
            try
            {
                McpMessage = JsonSerializer.Deserialize<McpMessage>(httpRequest.Body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                throw new ProtocolException($"Failed to parse MCP message from HTTP body: {ex.Message}", ex);
            }
        }
    }
}

/// <summary>
/// Factory for creating MCP HTTP pipeline filters
/// </summary>
public class McpHttpPipelineFilterFactory : PipelineFilterFactoryBase<McpHttpRequest>
{
    protected override IPipelineFilter<McpHttpRequest> Create()
    {
        return new McpHttpPipelineFilter();
    }
}