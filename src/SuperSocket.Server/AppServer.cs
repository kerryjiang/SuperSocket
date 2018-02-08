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

        public Listener[] Listeners { get; private set; }

        private IList<IDuplexPipeListener> _socketListeners;

        protected internal ILoggerFactory LoggerFactory { get; private set; }

        private ILogger _logger;

        private bool _initialized = false;

        private IAppSessionFactory _appSessionFactory;
        
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

            ConfigureListeners(Config);

            _appSessionFactory = new AppSessionFactory<TPackageInfo, TPipelineFilter>(packageHandler);

            return _initialized = true;
        }

        private void ConfigureListeners(ServerConfig serverConfig)
        {
            Listeners = serverConfig
                .Listeners
                .Select(l => new Listener(l)).ToArray();
        }

        public bool Start()
        {
            if (!_initialized)
                throw new Exception("The server has not been initialized successfully!");

            var listenSockets = _socketListeners = new List<IDuplexPipeListener>(Listeners.Length);
            
            foreach (var listener in Listeners)
            {
                var listenSocket = _serviceProvider.GetService<IDuplexPipeListener>();

                try
                {
                    listenSocket.Start(listener.EndPoint, HandleNewClient);
                    _logger.LogDebug($"Listen the endpoint {listener.EndPoint} suceeded.");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Listen the endpoint {listener.EndPoint} failed. {e.Message}", e);
                    continue;
                }
                
                listenSockets.Add(listenSocket);
            }

            if (!listenSockets.Any())
            {
                _logger.LogError($"No listener was started!");
                return false;
            }

            return true;
        }

        private Task HandleNewClient(IDuplexPipe pipe)
        {
            Interlocked.Increment(ref _sessionCount);
            var session = _appSessionFactory.Create(pipe);
            session.Closed += OnSessionClosed;
            Task.Run(async () =>  await ProcessRequest(session));
            return Task.CompletedTask;
        }

        private async Task ProcessRequest(IAppSession session)
        {
            await session.ProcessRequest();
        }

        private void OnSessionClosed(object sender, EventArgs e)
        {
            Interlocked.Decrement(ref _sessionCount);
        }

        public void Stop()
        {
            _logger.LogDebug("The server is stopping...");
            _cancellationTokenSource.Cancel();

            _logger.LogDebug("Waiting for all listeners to stop...");

            foreach (var l in _socketListeners)
            {
                l.Stop();
            }

            _logger.LogDebug("All listeners have stoppped.");
            _logger.LogDebug("The server stopped.");
        }
    }
}