using System.Collections.Generic;

namespace SuperSocket.MCP.Abstractions
{
    /// <summary>
    /// Registry for MCP handlers that can be shared across different transports
    /// </summary>
    public interface IMcpHandlerRegistry
    {
        /// <summary>
        /// Registers a tool handler
        /// </summary>
        /// <param name="name">Tool name</param>
        /// <param name="handler">Tool handler</param>
        void RegisterTool(string name, IMcpToolHandler handler);

        /// <summary>
        /// Registers a resource handler
        /// </summary>
        /// <param name="uri">Resource URI</param>
        /// <param name="handler">Resource handler</param>
        void RegisterResource(string uri, IMcpResourceHandler handler);

        /// <summary>
        /// Registers a prompt handler
        /// </summary>
        /// <param name="name">Prompt name</param>
        /// <param name="handler">Prompt handler</param>
        void RegisterPrompt(string name, IMcpPromptHandler handler);

        /// <summary>
        /// Gets all registered tool handlers
        /// </summary>
        /// <returns>Dictionary of tool handlers</returns>
        IReadOnlyDictionary<string, IMcpToolHandler> GetToolHandlers();

        /// <summary>
        /// Gets all registered resource handlers
        /// </summary>
        /// <returns>Dictionary of resource handlers</returns>
        IReadOnlyDictionary<string, IMcpResourceHandler> GetResourceHandlers();

        /// <summary>
        /// Gets all registered prompt handlers
        /// </summary>
        /// <returns>Dictionary of prompt handlers</returns>
        IReadOnlyDictionary<string, IMcpPromptHandler> GetPromptHandlers();

        /// <summary>
        /// Gets a specific tool handler
        /// </summary>
        /// <param name="name">Tool name</param>
        /// <returns>Tool handler if found, null otherwise</returns>
        IMcpToolHandler? GetToolHandler(string name);

        /// <summary>
        /// Gets a specific resource handler
        /// </summary>
        /// <param name="uri">Resource URI</param>
        /// <returns>Resource handler if found, null otherwise</returns>
        IMcpResourceHandler? GetResourceHandler(string uri);

        /// <summary>
        /// Gets a specific prompt handler
        /// </summary>
        /// <param name="name">Prompt name</param>
        /// <returns>Prompt handler if found, null otherwise</returns>
        IMcpPromptHandler? GetPromptHandler(string name);
    }
}