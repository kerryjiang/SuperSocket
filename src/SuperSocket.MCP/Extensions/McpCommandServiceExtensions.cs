using System;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Commands;
using SuperSocket.MCP.Models;

namespace SuperSocket.MCP.Extensions
{
    /// <summary>
    /// Extension methods for configuring MCP command-based servers
    /// </summary>
    public static class McpCommandServiceExtensions
    {
        /// <summary>
        /// Adds MCP command server services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="serverInfo">Server information</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddMcpCommandServices(this IServiceCollection services, McpServerInfo serverInfo)
        {
            if (serverInfo == null)
                throw new ArgumentNullException(nameof(serverInfo));

            // Register the server info
            services.AddSingleton(serverInfo);

            // Register the handler registry
            services.AddSingleton<IMcpHandlerRegistry, McpHandlerRegistry>();

            // Register the command dispatcher
            services.AddSingleton<McpCommandDispatcher>();

            // Register MCP commands
            services.AddScoped<InitializeCommand>();
            services.AddScoped<ListToolsCommand>();
            services.AddScoped<CallToolCommand>();
            services.AddScoped<ListResourcesCommand>();
            services.AddScoped<ReadResourceCommand>();
            services.AddScoped<ListPromptsCommand>();
            services.AddScoped<GetPromptCommand>();

            return services;
        }
    }
}