using SuperSocket.MCP.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;


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
    /// <param name="name">Prompt name</param>
    /// <param name="arguments">Prompt arguments</param>
    /// <returns>Prompt result</returns>
    Task<McpPromptResult> GetAsync(string name, Dictionary<string, object>? arguments = null);
}