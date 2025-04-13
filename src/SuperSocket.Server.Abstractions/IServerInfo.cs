using System;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Provides information about a server.
    /// </summary>
    public interface IServerInfo
    {
        /// <summary>
        /// Gets the name of the server.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the server options.
        /// </summary>
        ServerOptions Options { get; }

        /// <summary>
        /// Gets or sets the data context associated with the server.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        /// Gets the number of active sessions on the server.
        /// </summary>
        int SessionCount { get; }

        /// <summary>
        /// Gets the service provider for dependency injection.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the current state of the server.
        /// </summary>
        ServerState State { get; }
    }
}