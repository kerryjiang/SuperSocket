using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Config;
using SuperSocket.Channel;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace SuperSocket.Server
{
    public class AppServer
    {
        private IServiceCollection _serviceCollection;

        private IServiceProvider _serviceProvider;

        public ServerConfig Config { get; private set; }

        private long _sessionCount;

        /// <summary>
        /// Total session count
        /// </summary>
        /// <returns>total session count</returns>
        public long SessionCount
        {
            get
            {
                return _sessionCount;
            }
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Server's name
        /// </summary>
        /// <returns>the name of the server instance</returns>
        public string Name 
        {
            get { return Config.Name; }
        }

        private IList<ITransport> _transports;

        protected internal ILoggerFactory LoggerFactory { get; private set; }

        private ILogger _logger;

        private bool _initialized = false;

        private ITransportFactory _transportFactory;
        
        public bool Configure<TPackageInfo, TPipelineFilter>(IConfiguration config, IServiceCollection services = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (services == null)
            {
                services = new ServiceCollection();
            }

            if (config == null)
                throw new ArgumentNullException(nameof(config));            
            
            // prepare service collections
            _serviceCollection = services.AddOptions() // activate options
                .AddLogging() // add logging
                .Configure<ServerConfig>(config);

            // build service provider
            _serviceProvider = services.BuildServiceProvider();

            // get server config
            var serverConfigOption = _serviceProvider.GetService<IOptions<ServerConfig>>();

            if (serverConfigOption == null || serverConfigOption.Value == null)
            {
                throw new ArgumentException("Invalid configuration", nameof(config));
            }

            Config = serverConfigOption.Value;

            // initialize logger factory
            LoggerFactory = _serviceProvider
                    .GetService<ILoggerFactory>()
                    .AddConsole(LogLevel.Debug);

            _logger = LoggerFactory.CreateLogger("SocketServer");

            return _initialized = true;
        }



        public Task<bool> StartAsync()
        {
            if (!_initialized)
                throw new Exception("The server has not been initialized successfully!");
            
            throw new NotImplementedException();
        }
        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}