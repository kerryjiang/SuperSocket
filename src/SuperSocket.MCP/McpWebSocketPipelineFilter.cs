using System;
using System.Text.Json;
using SuperSocket.MCP.Models;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket;
using SuperSocket.Command;

namespace SuperSocket.MCP
{
    /// <summary>
    /// Pipeline filter for MCP protocol over WebSocket connections
    /// Converts WebSocket text frames containing JSON-RPC messages into MCP messages
    /// </summary>
    public class McpWebSocketPipelineFilter : IPackageMapper<WebSocketPackage, McpMessage>
    {
        /// <summary>
        /// Maps a WebSocket package to an MCP message
        /// </summary>
        /// <param name="package">The WebSocket package containing the MCP JSON message</param>
        /// <returns>Decoded MCP message or null if the package is not a text message</returns>
        public McpMessage Map(WebSocketPackage package)
        {
            // Only process text frames for MCP protocol
            if (package.OpCode != OpCode.Text)
            {
                return null!;
            }

            // Skip empty messages
            if (string.IsNullOrWhiteSpace(package.Message))
            {
                return null!;
            }

            try
            {
                // Parse JSON-RPC message from WebSocket text frame
                var message = JsonSerializer.Deserialize<McpMessage>(package.Message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                });

                return message ?? throw new InvalidOperationException("Failed to deserialize MCP message from WebSocket frame");
            }
            catch (JsonException ex)
            {
                throw new ProtocolException($"Failed to parse MCP JSON message from WebSocket: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ProtocolException($"Unexpected error parsing MCP WebSocket message: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// WebSocket-based MCP encoder that converts MCP messages to WebSocket text frames
    /// </summary>
    public class McpWebSocketEncoder : IPackageEncoder<McpMessage>
    {
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the McpWebSocketEncoder
        /// </summary>
        public McpWebSocketEncoder()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Encodes an MCP message into a WebSocket text frame
        /// </summary>
        /// <param name="writer">Buffer writer to write the encoded data</param>
        /// <param name="package">MCP message to encode</param>
        /// <returns>Number of bytes written</returns>
        public int Encode(System.Buffers.IBufferWriter<byte> writer, McpMessage package)
        {
            try
            {
                // Serialize MCP message to JSON
                var json = JsonSerializer.Serialize(package, _jsonOptions);
                
                // Create WebSocket text frame containing the JSON
                var webSocketPackage = new WebSocketPackage
                {
                    OpCode = OpCode.Text,
                    Message = json,
                    FIN = true // Always send complete messages
                };

                // Use the standard WebSocket encoder to encode the frame
                var webSocketEncoder = new WebSocketEncoder();
                return webSocketEncoder.Encode(writer, webSocketPackage);
            }
            catch (Exception ex)
            {
                throw new ProtocolException($"Failed to encode MCP message to WebSocket frame: {ex.Message}", ex);
            }
        }
    }
}