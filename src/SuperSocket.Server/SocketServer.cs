using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Base;

namespace SuperSocket.Server
{
    public class SocketServer
    {
        private IServiceCollection _serviceCollection;

        private IServiceProvider _serviceProvider;

        public ServerConfig Config { get; private set; }

        public string Name 
        {
            get { return Config.Name; }
        }

        protected internal ILoggerFactory LoggerFactory { get; private set; }

        public bool Configure(IConfiguration config)
        {
            var services = new ServiceCollection();
            return Configure(services, config);
        }

        public bool Configure(IServiceCollection services, IConfiguration config)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _serviceCollection = services.AddOptions()
                .AddLogging()
                .Configure<ServerConfig>(config);

            _serviceProvider = services.BuildServiceProvider();

            var serverConfigOption = _serviceProvider.GetService<IOptions<ServerConfig>>();

            if (serverConfigOption == null || serverConfigOption.Value == null)
            {
                throw new ArgumentException("Invalid configuration", nameof(config));
            }

            Config = serverConfigOption.Value;

            LoggerFactory = _serviceProvider
                    .GetService<ILoggerFactory>()
                    .AddConsole(LogLevel.Debug);

            return true;
        }
    }
}