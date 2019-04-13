using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    [Obsolete]
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
        public string Name => Options.Name;

        protected internal ILoggerFactory LoggerFactory { get; private set; }

        private ILogger _logger;

        private bool _configured;

        private int _sessionCount;

        private IList<IListener> _listeners;

        private Action<IAppSession> _sessionInitializer;

        private IMiddleware _middleware;

        public void UseMiddleware<TMiddleware>()
            where TMiddleware : IMiddleware
        {
            var middleware = _serviceProvider.GetService<TMiddleware>();

            if (_middleware == null)
                _middleware = middleware;
            else
                _middleware.Next = middleware;
        }        

        public bool Configure<TPackageInfo>(ServerOptions options, IServiceCollection services = null, IPipelineFilterFactory<TPackageInfo> pipelineFilterFactory = null, Func<IAppSession, TPackageInfo, Task> packageHandler = null)
            where TPackageInfo : class
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

            if (pipelineFilterFactory == null)
            {
                pipelineFilterFactory = _serviceProvider.GetService<IPipelineFilterFactory<TPackageInfo>>();
            }

            if (pipelineFilterFactory == null)
                throw new ArgumentNullException(nameof(pipelineFilterFactory));

            var listenerFactory = _serviceProvider.GetService<IListenerFactory>();

            if (listenerFactory == null)
                listenerFactory = new TcpSocketListenerFactory();

            _listeners = new List<IListener>();

            foreach (var l in options.Listeners)
            {
                var listener = listenerFactory.CreateListener<TPackageInfo>(l, LoggerFactory, pipelineFilterFactory);
                listener.NewClientAccepted += OnNewClientAccept;

                if (packageHandler != null)
                {
                    _sessionInitializer = (s) =>
                    {
                        if (s.Channel is IChannel<TPackageInfo> channel)
                        {
                            channel.PackageReceived += async (ch, p) =>
                            {
                                try
                                {
                                    await packageHandler(s, p);
                                }
                                catch (Exception e)
                                {
                                    OnSessionError(s, e);
                                }
                            };
                        }
                    };
                }

                _listeners.Add(listener);
            }

            return _configured = true;
        }

        public bool Configure<TPackageInfo, TPipelineFilter>(ServerOptions options, IServiceCollection services = null, Func<IAppSession, TPackageInfo, Task> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter: IPipelineFilter<TPackageInfo>, new()
        {
            return Configure(options, services, new DefaultPipelineFilterFactory<TPackageInfo, TPipelineFilter>(), packageHandler: packageHandler);
        }

        protected virtual void OnNewClientAccept(IListener listener, IChannel channel)
        {
            var session = new AppSession(this, channel);

            _sessionInitializer.Invoke(session);
            HandleSession(session).DoNotAwait();
        }

        private void RegisterMiddleware(IAppSession session)
        {
            var middleware = _middleware;

            while (middleware != null)
            {
                middleware.Register(this, session);
                middleware = middleware.Next;
            }
        }

        private async Task HandleSession(AppSession session)
        {
            Interlocked.Increment(ref _sessionCount);

            try
            {
                _logger.LogInformation($"A new session connected: {session.SessionID}");
                await session.Channel.StartAsync();
                _logger.LogInformation($"The session disconnected: {session.SessionID}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to handle the session {session.SessionID}.", e);
            }
            
            Interlocked.Decrement(ref _sessionCount);
        }

        protected virtual void OnSessionError(IAppSession session, Exception exception)
        {
            _logger.LogError($"Session[{session.SessionID}]: session exception.", exception);
        }

        public int SessionCount => _sessionCount;

        public async Task<bool> StartAsync()
        {
            await Task.Delay(0);

            if (!_configured)
                _logger.LogError("The server has not been initialized successfully!");

            var bound = 0;

            foreach (var listener in _listeners)
            {
                try
                {
                    listener.Start();
                    bound++;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to bind the transport {listener}.");
                }
            }

            if (bound == 0)
            {
                _logger.LogCritical("No transport bound successfully.");
                return false;
            }

            return true;
        }

        public async Task StopAsync()
        {
            var tasks = _listeners.Where(l => l.IsRunning).Select(l => l.StopAsync()).ToArray();
            await Task.WhenAll(tasks);
        }
    }
}