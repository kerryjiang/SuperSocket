using System;

namespace SuperSocket.Server.Abstractions.Host
{
    /// <summary>
    /// Represents a minimal API host builder.
    /// </summary>
    public interface IMinimalApiHostBuilder
    {
        /// <summary>
        /// Configures the host builder.
        /// </summary>
        void ConfigureHostBuilder();
    }
}