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

        protected internal ILoggerFactory LoggerFactory { get; private set; }

        private ILogger _logger;

        private bool _configured = false;

        private IList<IListener> _listeners;
        
        public bool Configure<TPackageInfo, TPipelineFilter>(ServerOptions options, IServiceCollection services = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Options = options;
                
            if (services == null)
                services = new ServiceCollection();

            // prepare service collections
            _serviceCollection = services.AddOptions(); // activate options     

            // build service provider
            _serviceProvider = services.BuildServiceProvider();

            // initialize logger factory
            LoggerFactory = _serviceProvider.GetService<ILoggerFactory>();

            _logger = LoggerFactory.CreateLogger("SuperSocket");

            var listenerFactory = _serviceProvider.GetService<IListenerFactory>();

            if (listenerFactory == null)
                listenerFactory = new TcpSocketListenerFactory();

            _listeners = new List<IListener>();

            foreach (var l in options.Listeners)
            {
                _listeners.Add(listenerFactory.CreateListener(l));
            }

            return _configured = true;
        }

        public int SessionCount { get; private set; }

        public async Task<bool> StartAsync()
        {
            await Task.Delay(0);

            if (!_configured)
                _logger.LogError("The server has not been initialized successfully!");

            var binded = 0;

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.Start();
                    binded++;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to bind the transport {listener.ToString()}.");
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
            var tasks = _listeners.Select(l => l.StopAsync()).ToArray();
            await Task.WhenAll(tasks);
        }
    }
}