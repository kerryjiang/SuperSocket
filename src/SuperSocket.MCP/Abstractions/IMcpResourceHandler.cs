using System.Threading.Tasks;

namespace SuperSocket.MCP.Abstractions;


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