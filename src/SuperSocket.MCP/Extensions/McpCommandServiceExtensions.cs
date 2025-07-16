using System;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using SuperSocket.MCP.Abstractions;
using SuperSocket.MCP.Commands;
using SuperSocket.MCP.Models;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.MCP.Extensions
{
    /// <summary>
    /// Extension methods for configuring MCP commands with SuperSocket
    /// </summary>
    public static class McpCommandServiceExtensions
    {
        /// <summary>
        /// Adds MCP command services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="serverInfo">MCP server information</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMcpCommandServices(this IServiceCollection services, McpServerInfo serverInfo)
        {
            if (serverInfo == null)
                throw new ArgumentNullException(nameof(serverInfo));

            // Register server info
            services.AddSingleton(serverInfo);

            // Register handler registry
            services.AddSingleton<IMcpHandlerRegistry, McpHandlerRegistry>();

            // Register individual MCP commands
            services.AddScoped<InitializeCommand>();
            services.AddScoped<ListToolsCommand>();
            services.AddScoped<CallToolCommand>();
            services.AddScoped<ListResourcesCommand>();
            services.AddScoped<ReadResourceCommand>();
            services.AddScoped<ListPromptsCommand>();
            services.AddScoped<GetPromptCommand>();
            services.AddScoped<InitializedCommand>();

            return services;
        }

        /// <summary>
        /// Configures the SuperSocket host builder to use MCP commands
        /// </summary>
        /// <param name="builder">SuperSocket host builder</param>
        /// <param name="serverInfo">MCP server information</param>
        /// <returns>Configured host builder</returns>
        public static ISuperSocketHostBuilder<McpMessage> UseMcpCommands(this ISuperSocketHostBuilder<McpMessage> builder, McpServerInfo serverInfo)
        {
            if (serverInfo == null)
                throw new ArgumentNullException(nameof(serverInfo));

            return builder
                .ConfigureServices((hostCtx, services) =>
                {
                    services.AddMcpCommandServices(serverInfo);
                })
                .UseCommand((commandOptions) =>
                {
                    // Register all MCP commands
                    commandOptions.AddCommand<InitializeCommand>();
                    commandOptions.AddCommand<ListToolsCommand>();
                    commandOptions.AddCommand<CallToolCommand>();
                    commandOptions.AddCommand<ListResourcesCommand>();
                    commandOptions.AddCommand<ReadResourceCommand>();
                    commandOptions.AddCommand<ListPromptsCommand>();
                    commandOptions.AddCommand<GetPromptCommand>();
                    commandOptions.AddCommand<InitializedCommand>();
                });
        }
    }
}