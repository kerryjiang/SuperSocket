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
    public class SuperSocketServer : IServer
    {
        private IServiceCollection _serviceCollection;

        private IServiceProvider _serviceProvider;

        public ServerOptions Options { get; private set; }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Server's name
        /// </summary>
        /// <returns>the name of the server instance</returns>
        public string Name 
        {
            get { return Options.Name; }
        }

        private IList<ITransport> _transports;

        protected internal ILoggerFactory LoggerFactory { get; private set; }

        private ILogger _logger;

        private bool _configured = false;

        private ITransportFactory _transportFactory;
        
        public bool Configure<TPackageInfo, TPipelineFilter>(ServerOptions options, ITransportFactory transportFactory, IServiceCollection services = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Options = options;

            if (transportFactory == null)
                throw new ArgumentNullException(nameof(transportFactory));

            _transportFactory = transportFactory;
                
            if (services == null)
                services = new ServiceCollection();
            
            // prepare service collections
            _serviceCollection = services.AddOptions() // activate options
                .AddLogging();// add logging

            // build service provider
            _serviceProvider = services.BuildServiceProvider();

            // initialize logger factory
            LoggerFactory = _serviceProvider
                    .GetService<ILoggerFactory>()
                    .AddConsole(LogLevel.Debug);

            _logger = LoggerFactory.CreateLogger("SuperSocket");

            _transports = new List<ITransport>();

            foreach (var l in options.Listeners)
            {
                _transports.Add(_transportFactory.Create(new EndPointInformation(l), new ConnectionDispatcher()));
            }

            return _configured = true;
        }

        public async Task<bool> StartAsync()
        {
            if (!_configured)
                throw new Exception("The server has not been initialized successfully!");

            foreach (var transport in _transports)
            {
                await transport.BindAsync();
            }
        }

        public async Task StopAsync()
        {
            foreach (var transport in _transports)
            {
                await transport.UnbindAsync();
            }
        }
    }
}