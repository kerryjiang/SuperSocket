using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using SuperSocket.MCP.Models;
using SuperSocket.ProtoBase;

namespace SuperSocket.MCP;

/// <summary>
/// Pipeline filter for MCP protocol using Content-Length header format
/// </summary>
public class McpPipelineFilter : PipelineFilterBase<McpMessage>
{
    private const string ContentLengthHeader = "Content-Length: ";
    private const string ContentTypeHeader = "Content-Type: ";
    private const string Separator = "\r\n";
    private const string HeaderEnd = "\r\n\r\n";

    private int _expectedContentLength = -1;
    private bool _headerParsed = false;

    public override McpMessage Filter(ref SequenceReader<byte> reader)
    {
        if (!_headerParsed)
        {
            if (!TryParseHeaders(ref reader, out var contentLength))
            {
                return null!; // Need more data
            }

            _expectedContentLength = contentLength;
            _headerParsed = true;
        }

        // Check if we have enough data for the content
        if (reader.Remaining < _expectedContentLength)
        {
            return null!; // Need more data
        }

        // Extract the JSON content
        var jsonBuffer = reader.Sequence.Slice(reader.Position, _expectedContentLength);
        var jsonBytes = jsonBuffer.ToArray();
        var jsonString = Encoding.UTF8.GetString(jsonBytes);

        try
        {
            var message = JsonSerializer.Deserialize<McpMessage>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Advance the reader
            reader.Advance(_expectedContentLength);

            // Reset for next message
            ResetState();

            return message ?? throw new InvalidOperationException("Failed to deserialize MCP message");
        }
        catch (JsonException ex)
        {
            ResetState();
            throw new ProtocolException($"Failed to parse JSON: {ex.Message}", ex);
        }
    }

    private bool TryParseHeaders(ref SequenceReader<byte> reader, out int contentLength)
    {
        contentLength = 0;

        // Look for header end marker
        var headerEndBytes = Encoding.UTF8.GetBytes(HeaderEnd);
        if (!reader.TryReadTo(out ReadOnlySequence<byte> headerBuffer, headerEndBytes, advancePastDelimiter: false))
        {
            return false; // Headers not complete
        }

        // Extract headers
        var headerBytes = headerBuffer.ToArray();
        var headerString = Encoding.UTF8.GetString(headerBytes);

        // Parse Content-Length header
        var lines = headerString.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith(ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
            {
                var lengthStr = line.Substring(ContentLengthHeader.Length).Trim();
                if (int.TryParse(lengthStr, out contentLength))
                {
                    // Advance reader past headers
                    reader.Advance(headerBuffer.Length + headerEndBytes.Length);
                    return true;
                }
            }
        }

        throw new ProtocolException("Content-Length header not found or invalid");
    }

    private void ResetState()
    {
        _expectedContentLength = -1;
        _headerParsed = false;
    }

    public override void Reset()
    {
        base.Reset();
        ResetState();
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