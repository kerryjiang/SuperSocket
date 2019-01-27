using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.AspNetCore.Hosting;
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

        private ISuperSocketConnectionDispatcher _connectionDispatcher;

        private ITransportFactory _transportFactory;
        
        public bool Configure<TPackageInfo, TPipelineFilter>(ServerOptions options, IServiceCollection services = null, ITransportFactory transportFactory = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Options = options;


            _transportFactory = transportFactory;
                
            if (services == null)
                services = new ServiceCollection();

            // prepare service collections
            _serviceCollection = services.AddOptions(); // activate options     

            _serviceCollection.AddSingleton<IApplicationLifetime, SuperSocketApplicationLifetime>();

            // build service provider
            _serviceProvider = services.BuildServiceProvider();

            // initialize logger factory
            LoggerFactory = _serviceProvider.GetService<ILoggerFactory>();

            _logger = LoggerFactory.CreateLogger("SuperSocket");

            if (_transportFactory == null)
            {
                _transportFactory = _serviceProvider.GetRequiredService<ITransportFactory>();
                
                if (_transportFactory == null)
                {
                    throw new ArgumentNullException(nameof(transportFactory));
                }
            }

            _connectionDispatcher = new ConnectionDispatcher<TPackageInfo, TPipelineFilter>();

            _transports = new List<ITransport>();

            foreach (var l in options.Listeners)
            {
                _transports.Add(_transportFactory.Create(new SuperSocketEndPointInformation(l), _connectionDispatcher));
            }

            return _configured = true;
        }

        public int SessionCount
        {
            get { return _connectionDispatcher.SessionCount; }
        }

        public async Task<bool> StartAsync()
        {
            if (!_configured)
                _logger.LogError("The server has not been initialized successfully!");

            var binded = 0;

            foreach (var transport in _transports)
            {
                try
                {
                    await transport.BindAsync();
                    binded++;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to bind the transport {transport.ToString()}.");
                }
            }

            if (binded == 0)
            {
                _logger.LogCritical("No transport binded successfully.");
                return false;
            }

            return true;
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