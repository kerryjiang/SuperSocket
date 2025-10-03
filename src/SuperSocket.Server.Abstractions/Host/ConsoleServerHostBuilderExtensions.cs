using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.Server
{
    /// <summary>
    /// Provides extension methods for configuring a console server host builder.
    /// </summary>
    public static class ConsoleServerHostBuilderExtensions
    {
        /// <summary>
        /// Configures the host builder to use console (stdin/stdout) for communication.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder UseConsole(this ISuperSocketHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IConnectionListenerFactory, ConsoleConnectionListenerFactory>();
            }) as ISuperSocketHostBuilder;
        }

        /// <summary>
        /// Configures the host builder to use console (stdin/stdout) for communication with a specific package type.
        /// </summary>
        /// <typeparam name="TReceivePackage">The type of the package to receive.</typeparam>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static ISuperSocketHostBuilder<TReceivePackage> UseConsole<TReceivePackage>(this ISuperSocketHostBuilder<TReceivePackage> hostBuilder)
        {
            return (hostBuilder as ISuperSocketHostBuilder).UseConsole() as ISuperSocketHostBuilder<TReceivePackage>;
        }
    }
}