using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class SuperSocketService<TReceivePackageInfo> : IHostedService, IServer, IChannelRegister, ILoggerAccessor
    {
        private readonly IServiceProvider _serviceProvider;

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        private readonly IOptions<ServerOptions> _serverOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        internal protected ILogger Logger
        {
            get { return _logger; }
        }

        ILogger ILoggerAccessor.Logger
        {
            get { return _logger; }
        }

        private IPipelineFilterFactory<TReceivePackageInfo> _pipelineFilterFactory;
        private IChannelCreatorFactory _channelCreatorFactory;
        private List<IChannelCreator> _channelCreators;
        private IPackageHandlingScheduler<TReceivePackageInfo> _packageHandlingScheduler;
        
        public string Name { get; }

        private int _sessionCount;

        public int SessionCount => _sessionCount;

        private ISessionFactory _sessionFactory;

        private IMiddleware[] _middlewares;

        protected IMiddleware[] Middlewares
        {
            get { return _middlewares; }
        }

        private ServerState _state = ServerState.None;

        public ServerState State
        {
            get { return _state; }
        }

        public object DataContext { get; set; }

        private SessionHandlers _sessionHandlers;

        public SuperSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (serverOptions == null)
                throw new ArgumentNullException(nameof(serverOptions));

            Name = serverOptions.Value.Name; 
            _serverOptions = serverOptions;                       
            _serviceProvider = serviceProvider;
            _pipelineFilterFactory = GetPipelineFilterFactory();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = _loggerFactory.CreateLogger("SuperSocketService");
            _channelCreatorFactory = serviceProvider.GetService<IChannelCreatorFactory>() ?? new TcpChannelCreatorFactory(serviceProvider);
            _sessionHandlers = serviceProvider.GetService<SessionHandlers>();          
            // initialize session factory
            _sessionFactory = serviceProvider.GetService<ISessionFactory>() ?? new DefaultSessionFactory();

            InitializeMiddlewares();

            var packageHandler = serviceProvider.GetService<IPackageHandler<TReceivePackageInfo>>()
                ?? _middlewares.OfType<IPackageHandler<TReceivePackageInfo>>().FirstOrDefault();

            if (packageHandler == null)
            {
                Logger.LogWarning("The PackageHandler cannot be found.");
            }
            else
            {
                var errorHandler = serviceProvider.GetService<Func<IAppSession, PackageHandlingException<TReceivePackageInfo>, ValueTask<bool>>>()
                ?? OnSessionErrorAsync;

                _packageHandlingScheduler = serviceProvider.GetService<IPackageHandlingScheduler<TReceivePackageInfo>>()
                    ?? new SerialPackageHandlingScheduler<TReceivePackageInfo>();
                _packageHandlingScheduler.Initialize(packageHandler, errorHandler);
            }
        }

        private void InitializeMiddlewares()
        {
            _middlewares = _serviceProvider.GetServices<IMiddleware>()
                .OrderBy(m => m.Order)
                .ToArray();

            foreach (var m in _middlewares)
            {
                m.Start(this);
            }
        }

        private void ShutdownMiddlewares()
        {
            foreach (var m in _middlewares)
            {
                try
                {
                    m.Shutdown(this);
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"The exception was thrown from the middleware {m.GetType().Name} when it is being shutdown.");
                }                
            }
        }

        protected virtual IPipelineFilterFactory<TReceivePackageInfo> GetPipelineFilterFactory()
        {
            return _serviceProvider.GetRequiredService<IPipelineFilterFactory<TReceivePackageInfo>>();
        }

        private bool AddChannelCreator(ListenOptions listenOptions, ServerOptions serverOptions)
        {
            var listener = _channelCreatorFactory.CreateChannelCreator<TReceivePackageInfo>(listenOptions, serverOptions, _loggerFactory, _pipelineFilterFactory);
            listener.NewClientAccepted += OnNewClientAccept;

            if (!listener.Start())
            {
                _logger.LogError($"Failed to listen {listener}.");
                return false;
            }

            _logger.LogInformation($"The listener [{listener}] has been started.");
            _channelCreators.Add(listener);
            return true;
        }

        private Task<bool> StartListenAsync(CancellationToken cancellationToken)
        {
            _channelCreators = new List<IChannelCreator>();

            var serverOptions = _serverOptions.Value;

            if (serverOptions.Listeners != null && serverOptions.Listeners.Any())
            {
                foreach (var l in serverOptions.Listeners)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!AddChannelCreator(l, serverOptions))
                    {
                        _logger.LogError($"Failed to listen {l}.");
                        continue;
                    }
                }
            }
            else
            {
                _logger.LogWarning("No listener was defined, so this server only can accept connections from the ActiveConnect.");

                if (!AddChannelCreator(null, serverOptions))
                {
                    _logger.LogError($"Failed to add the channel creator.");
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        protected virtual void OnNewClientAccept(IChannelCreator listener, IChannel channel)
        {
            AcceptNewChannel(channel);
        }

        private void AcceptNewChannel(IChannel channel)
        {
            var session = _sessionFactory.Create() as AppSession;
            HandleSession(session, channel).DoNotAwait();
        }

        async Task IChannelRegister.RegisterChannel(object connection)
        {
            var channel = await _channelCreators.FirstOrDefault().CreateChannel(connection);
            AcceptNewChannel(channel);
        }

        protected virtual object CreatePipelineContext(IAppSession session)
        {
            return session;
        }

        private async ValueTask<bool> InitializeSession(IAppSession session, IChannel channel)
        {
            session.Initialize(this, channel);

            if (channel is IPipeChannel pipeChannel)
            {
                pipeChannel.PipelineFilter.Context = CreatePipelineContext(session);
            }

            var middlewares = _middlewares;

            if (middlewares != null && middlewares.Length > 0)
            {
                for (var i = 0; i < middlewares.Length; i++)
                {
                    var middleware = middlewares[i];

                    if (!await middleware.RegisterSession(session))
                    {
                        _logger.LogWarning($"A session from {session.RemoteEndPoint} was rejected by the middleware {middleware.GetType().Name}.");
                        return false;
                    }
                }
            }

            return true;
        }


        protected virtual ValueTask OnSessionConnectedAsync(IAppSession session)
        {
            var connectedHandler = _sessionHandlers?.Connected;

            if (connectedHandler != null)
                return connectedHandler.Invoke(session);

            return new ValueTask();
        }

        protected virtual ValueTask OnSessionClosedAsync(IAppSession session)
        {
            var closedHandler = _sessionHandlers?.Closed;

            if (closedHandler != null)
                return closedHandler.Invoke(session);

            return new ValueTask();
        }

        protected virtual async ValueTask FireSessionConnectedEvent(AppSession session)
        {
            if (session is IHandshakeRequiredSession hanshakeSession)
            {
                if (!hanshakeSession.Handshaked)
                    return;
            }

            _logger.LogInformation($"A new session connected: {session.SessionID}");

            try
            {
                Interlocked.Increment(ref _sessionCount);
                await session.FireSessionConnectedAsync();
                await OnSessionConnectedAsync(session);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There is one exception thrown from the event handler of SessionConnected.");
            }
        }

        protected virtual async ValueTask FireSessionClosedEvent(AppSession session)
        {
            if (session is IHandshakeRequiredSession hanshakeSession)
            {
                if (!hanshakeSession.Handshaked)
                    return;
            }

            _logger.LogInformation($"The session disconnected: {session.SessionID}");

            try
            {
                Interlocked.Decrement(ref _sessionCount);
                await session.FireSessionClosedAsync(EventArgs.Empty);
                await OnSessionClosedAsync(session); 
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "There is one exception thrown from the event of OnSessionClosed.");
            }
        }

        private async ValueTask HandleSession(AppSession session, IChannel channel)
        {
            if (!await InitializeSession(session, channel))
                return;

            try
            {
                channel.Start();
                
                await FireSessionConnectedEvent(session);

                var packageChannel = channel as IChannel<TReceivePackageInfo>;
                var packageHandlingScheduler = _packageHandlingScheduler;

                await foreach (var p in packageChannel.RunAsync())
                {
                    await packageHandlingScheduler.HandlePackage(session, p);   
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to handle the session {session.SessionID}.");
            }
            finally
            {
                await FireSessionClosedEvent(session);
            }
        }

        protected virtual ValueTask<bool> OnSessionErrorAsync(IAppSession session, PackageHandlingException<TReceivePackageInfo> exception)
        {
            _logger.LogError(exception, $"Session[{session.SessionID}]: session exception.");
            return new ValueTask<bool>(true);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.None && state != ServerState.Stopped)
            {
                throw new InvalidOperationException($"The server cannot be started right now, because its state is {state}.");
            }

            _state = ServerState.Starting;

            await StartListenAsync(cancellationToken);

            _state = ServerState.Started;

            try
            {
                await OnStartedAsync();
            }
            catch(Exception e)
            {
                _logger.LogError(e, "There is one exception thrown from the method OnStartedAsync().");
            }
        }

        protected virtual ValueTask OnStartedAsync()
        {
            return new ValueTask();
        }

        protected virtual ValueTask OnStopAsync()
        {
            return new ValueTask();
        }

        private async Task StopListener(IChannelCreator listener)
        {
            await listener.StopAsync();
            _logger.LogInformation($"The listener [{listener}] has been stopped.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.Started)
            {
                throw new InvalidOperationException($"The server cannot be stopped right now, because its state is {state}.");
            }

            _state = ServerState.Stopping;

            var tasks = _channelCreators.Where(l => l.IsRunning).Select(l => StopListener(l))
                .Union(new Task[] { Task.Run(ShutdownMiddlewares) });

            await Task.WhenAll(tasks);

            try
            {
                await OnStopAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There is an exception thrown from the method OnStopAsync().");
            }

            _state = ServerState.Stopped;
        }

        async Task<bool> IServer.StartAsync()
        {
            await StartAsync(CancellationToken.None);
            return true;
        }

        async Task IServer.StopAsync()
        {
            await StopAsync(CancellationToken.None);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync(true);

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        if (_state == ServerState.Started)
                        {
                            await StopAsync(CancellationToken.None);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to stop the server");
                    }
                }

                disposedValue = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            DisposeAsync(disposing).GetAwaiter().GetResult();
        }

        void IDisposable.Dispose()
        {
            DisposeAsync(true).GetAwaiter().GetResult();
        }

        #endregion
    }
}
