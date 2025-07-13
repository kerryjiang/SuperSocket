using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SuperSocket.MCP.Abstractions;

namespace SuperSocket.MCP
{
    /// <summary>
    /// Default implementation of MCP handler registry
    /// </summary>
    public class McpHandlerRegistry : IMcpHandlerRegistry
    {
        private readonly ILogger<McpHandlerRegistry> _logger;
        private readonly ConcurrentDictionary<string, IMcpToolHandler> _toolHandlers = new();
        private readonly ConcurrentDictionary<string, IMcpResourceHandler> _resourceHandlers = new();
        private readonly ConcurrentDictionary<string, IMcpPromptHandler> _promptHandlers = new();

        /// <summary>
        /// Initializes a new instance of the McpHandlerRegistry class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public McpHandlerRegistry(ILogger<McpHandlerRegistry> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void RegisterTool(string name, IMcpToolHandler handler)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _toolHandlers.TryAdd(name, handler);
            _logger.LogInformation("Registered tool: {ToolName}", name);
        }

        /// <inheritdoc />
        public void RegisterResource(string uri, IMcpResourceHandler handler)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _resourceHandlers.TryAdd(uri, handler);
            _logger.LogInformation("Registered resource: {ResourceUri}", uri);
        }

        /// <inheritdoc />
        public void RegisterPrompt(string name, IMcpPromptHandler handler)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _promptHandlers.TryAdd(name, handler);
            _logger.LogInformation("Registered prompt: {PromptName}", name);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IMcpToolHandler> GetToolHandlers()
        {
            return _toolHandlers;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IMcpResourceHandler> GetResourceHandlers()
        {
            return _resourceHandlers;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IMcpPromptHandler> GetPromptHandlers()
        {
            return _promptHandlers;
        }

        /// <inheritdoc />
        public IMcpToolHandler? GetToolHandler(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            _toolHandlers.TryGetValue(name, out var handler);
            return handler;
        }

        /// <inheritdoc />
        public IMcpResourceHandler? GetResourceHandler(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;

            _resourceHandlers.TryGetValue(uri, out var handler);
            return handler;
        }

        /// <inheritdoc />
        public IMcpPromptHandler? GetPromptHandler(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            _promptHandlers.TryGetValue(name, out var handler);
            return handler;
        }
    }
}