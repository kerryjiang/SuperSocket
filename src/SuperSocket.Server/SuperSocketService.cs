using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static SuperSocket.Extensions;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.Server.Connection;
using SuperSocket.ProtoBase.ProxyProtocol;

namespace SuperSocket.Server
{
    /// <summary>
    /// Represents a SuperSocket service that handles connections and sessions.
    /// </summary>
    /// <typeparam name="TReceivePackageInfo">The type of the package information received.</typeparam>
    public class SuperSocketService<TReceivePackageInfo> : ISuperSocketHostedService
    {
        /// <summary>
        /// The service provider used for dependency injection.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the service provider for dependency injection.
        /// </summary>
        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        /// <summary>
        /// Gets the server options for configuration.
        /// </summary>
        public ServerOptions Options { get; }
        
        /// <summary>
        /// The logger factory used to create loggers.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;
        
        /// <summary>
        /// The logger instance for this service.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the logger for this service.
        /// </summary>
        internal protected ILogger Logger
        {
            get { return _logger; }
        }

        /// <summary>
        /// Gets the logger for the service.
        /// </summary>
        ILogger ILoggerAccessor.Logger
        {
            get { return _logger; }
        }

        /// <summary>
        /// The pipeline filter factory for processing received data.
        /// </summary>
        private IPipelineFilterFactory<TReceivePackageInfo> _pipelineFilterFactory;
        
        /// <summary>
        /// The factory for creating connection listeners.
        /// </summary>
        private IConnectionListenerFactory _connectionListenerFactory;
        
        /// <summary>
        /// The list of active connection listeners.
        /// </summary>
        private List<IConnectionListener> _connectionListeners;
        
        /// <summary>
        /// The scheduler for handling received packages.
        /// </summary>
        private IPackageHandlingScheduler<TReceivePackageInfo> _packageHandlingScheduler;
        
        /// <summary>
        /// The accessor for the current package handling context.
        /// </summary>
        private IPackageHandlingContextAccessor<TReceivePackageInfo> _packageHandlingContextAccessor;

        /// <summary>
        /// Gets the name of the server.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The current count of active sessions.
        /// </summary>
        private int _sessionCount;

        /// <summary>
        /// Gets the current session count.
        /// </summary>
        public int SessionCount => _sessionCount;

        /// <summary>
        /// The factory for creating new sessions.
        /// </summary>
        private ISessionFactory _sessionFactory;

        /// <summary>
        /// The array of configured middlewares.
        /// </summary>
        private IMiddleware[] _middlewares;

        /// <summary>
        /// Gets the array of configured middlewares.
        /// </summary>
        protected IMiddleware[] Middlewares
        {
            get { return _middlewares; }
        }

        /// <summary>
        /// The current state of the server.
        /// </summary>
        private ServerState _state = ServerState.None;

        /// <summary>
        /// Gets the current state of the server.
        /// </summary>
        public ServerState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets or sets the data context for the server.
        /// </summary>
        public object DataContext { get; set; }

        /// <summary>
        /// The handlers for session events.
        /// </summary>
        private SessionHandlers _sessionHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperSocketService{TReceivePackageInfo}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="serverOptions">The server options for configuration.</param>
        public SuperSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (serverOptions == null)
                throw new ArgumentNullException(nameof(serverOptions));

            Name = serverOptions.Value.Name;
            Options = serverOptions.Value;
            _serviceProvider = serviceProvider;
            _pipelineFilterFactory = GetPipelineFilterFactory();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = _loggerFactory.CreateLogger("SuperSocketService");
            _connectionListenerFactory = serviceProvider.GetService<IConnectionListenerFactory>();
            _sessionHandlers = serviceProvider.GetService<SessionHandlers>();
            _sessionFactory = serviceProvider.GetService<ISessionFactory>();
            _packageHandlingContextAccessor = serviceProvider.GetService<IPackageHandlingContextAccessor<TReceivePackageInfo>>();
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

        /// <summary>
        /// Gets the pipeline filter factory to be used for processing received data.
        /// </summary>
        /// <returns>A configured pipeline filter factory.</returns>
        protected virtual IPipelineFilterFactory<TReceivePackageInfo> GetPipelineFilterFactory()
        {
            var filterFactory = _serviceProvider.GetRequiredService<IPipelineFilterFactory<TReceivePackageInfo>>();

            if (Options.EnableProxyProtocol)
                filterFactory = new ProxyProtocolPipelineFilterFactory<TReceivePackageInfo>(filterFactory);

            return filterFactory;
        }

        /// <summary>
        /// Adds a new connection listener using the specified options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener, or null for default options.</param>
        /// <param name="serverOptions">The server options for configuration.</param>
        /// <returns>True if the listener was successfully created and started; otherwise, false.</returns>
        private bool AddConnectionListener(ListenOptions listenOptions, ServerOptions serverOptions)
        {
            var listener = _connectionListenerFactory.CreateConnectionListener(listenOptions, serverOptions, _loggerFactory);
            listener.NewConnectionAccept += OnNewConnectionAccept;

            if (!listener.Start())
            {
                _logger.LogError($"Failed to listen {listener}.");
                return false;
            }

            _logger.LogInformation($"The listener [{listener}] has been started.");
            _connectionListeners.Add(listener);
            return true;
        }

        /// <summary>
        /// Starts listening for incoming connections.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether any listeners were started successfully.</returns>
        private Task<bool> StartListenAsync(CancellationToken cancellationToken)
        {
            _connectionListeners = new List<IConnectionListener>();

            var serverOptions = Options;

            if (serverOptions.Listeners != null && serverOptions.Listeners.Any())
            {
                foreach (var l in serverOptions.Listeners)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!AddConnectionListener(l, serverOptions))
                    {
                        continue;
                    }
                }
            }
            else
            {
                _logger.LogWarning("No listener was defined, so this server only can accept connections from the ActiveConnect.");

                if (!AddConnectionListener(null, serverOptions))
                {
                    _logger.LogError($"Failed to add the connection creator.");
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(_connectionListeners.Any());
        }

        /// <summary>
        /// Called when a new connection is accepted by a listener.
        /// </summary>
        /// <param name="listenOptions">The options for the listener that accepted the connection.</param>
        /// <param name="connection">The newly established connection.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual ValueTask OnNewConnectionAccept(ListenOptions listenOptions, IConnection connection)
        {
            return AcceptNewConnection(connection);
        }

        /// <summary>
        /// Accepts a new connection and creates a session for it.
        /// </summary>
        /// <param name="connection">The newly established connection.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private ValueTask AcceptNewConnection(IConnection connection)
        {
            var session = _sessionFactory.Create() as AppSession;
            return HandleSession(session, connection);
        }

        /// <summary>
        /// Registers a new connection source with the server.
        /// </summary>
        /// <param name="connectionSource">The source of the connection to register.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        async Task IConnectionRegister.RegisterConnection(object connectionSource)
        {
            var connectionListener = _connectionListeners.FirstOrDefault();
#if NET6_0_OR_GREATER
            using var cts = CancellationTokenSourcePool.Shared.Rent(connectionListener.Options.ConnectionAcceptTimeOut);
#else
            using var cts = new CancellationTokenSource(connectionListener.Options.ConnectionAcceptTimeOut);
#endif
            var connection = await connectionListener.ConnectionFactory.CreateConnection(connectionSource, cts.Token);
            await AcceptNewConnection(connection);
        }

        /// <summary>
        /// Creates a context for the pipeline filter based on the session.
        /// </summary>
        /// <param name="session">The session for which to create the context.</param>
        /// <returns>The created pipeline context.</returns>
        protected virtual object CreatePipelineContext(IAppSession session)
        {
            return session;
        }

        #region Middlewares

        /// <summary>
        /// Initializes the middleware components from the service provider.
        /// </summary>
        private void InitializeMiddlewares()
        {
            _middlewares = _serviceProvider.GetServices<IMiddleware>()
                .OrderBy(m => m.Order)
                .ToArray();
        }

        /// <summary>
        /// Shuts down all middleware components.
        /// </summary>
        private void ShutdownMiddlewares()
        {
            foreach (var m in _middlewares)
            {
                try
                {
                    m.Shutdown(this);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"The exception was thrown from the middleware {m.GetType().Name} when it is being shutdown.");
                }
            }
        }

        /// <summary>
        /// Registers a session with all middleware components.
        /// </summary>
        /// <param name="session">The session to register.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the registration was successful with all middlewares.</returns>
        private async ValueTask<bool> RegisterSessionInMiddlewares(IAppSession session)
        {
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

        /// <summary>
        /// Unregisters a session from all middleware components.
        /// </summary>
        /// <param name="session">The session to unregister.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async ValueTask UnRegisterSessionFromMiddlewares(IAppSession session)
        {
            var middlewares = _middlewares;

            if (middlewares != null && middlewares.Length > 0)
            {
                for (var i = 0; i < middlewares.Length; i++)
                {
                    var middleware = middlewares[i];

                    try
                    {
                        if (!await middleware.UnRegisterSession(session))
                        {
                            _logger.LogWarning($"The session from {session.RemoteEndPoint} was failed to be unregistered from the middleware {middleware.GetType().Name}.");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"An unhandled exception occured when the session from {session.RemoteEndPoint} was being unregistered from the middleware {nameof(RegisterSessionInMiddlewares)}.");
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a session with the given connection and registers it with middlewares.
        /// </summary>
        /// <param name="session">The session to initialize.</param>
        /// <param name="connection">The connection to associate with the session.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether initialization was successful.</returns>
        private async ValueTask<bool> InitializeSession(IAppSession session, IConnection connection)
        {
            session.Initialize(this, connection);

            var middlewares = _middlewares;

            try
            {
                if (!await RegisterSessionInMiddlewares(session))
                {
                    session.CloseAsync(CloseReason.Rejected).DoNotAwait();
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An unhandled exception occured in {nameof(RegisterSessionInMiddlewares)}.");
                return false;
            }

            connection.Closed += (s, e) => OnConnectionClosed(session, e);
            return true;
        }

        /// <summary>
        /// Called when a session is connected and has been initialized.
        /// </summary>
        /// <param name="session">The connected session.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual ValueTask OnSessionConnectedAsync(IAppSession session)
        {
            var connectedHandler = _sessionHandlers?.Connected;

            if (connectedHandler != null)
                return connectedHandler.Invoke(session);

            return new ValueTask();
        }

        /// <summary>
        /// Handles the closing of a connection.
        /// </summary>
        /// <param name="session">The session associated with the closed connection.</param>
        /// <param name="e">The event arguments containing the reason for closure.</param>
        private void OnConnectionClosed(IAppSession session, CloseEventArgs e)
        {
            FireSessionClosedEvent(session as AppSession, e.Reason).DoNotAwait();
        }

        /// <summary>
        /// Called when a session is closed.
        /// </summary>
        /// <param name="session">The closed session.</param>
        /// <param name="e">The event arguments containing the reason for closure.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual ValueTask OnSessionClosedAsync(IAppSession session, CloseEventArgs e)
        {
            var closedHandler = _sessionHandlers?.Closed;

            if (closedHandler != null)
                return closedHandler.Invoke(session, e);

#if NETSTANDARD2_1
            return GetCompletedTask();
#else
            return ValueTask.CompletedTask;
#endif
        }

        /// <summary>
        /// Fires the session connected event for a session.
        /// </summary>
        /// <param name="session">The connected session.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async ValueTask FireSessionConnectedEvent(AppSession session)
        {
            if (session is IHandshakeRequiredSession handshakeSession)
            {
                if (!handshakeSession.Handshaked)
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

        /// <summary>
        /// Fires the session closed event for a session.
        /// </summary>
        /// <param name="session">The closed session.</param>
        /// <param name="reason">The reason for the session closure.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async ValueTask FireSessionClosedEvent(AppSession session, CloseReason reason)
        {
            if (session is IHandshakeRequiredSession handshakeSession)
            {
                if (!handshakeSession.Handshaked)
                    return;
            }

            await UnRegisterSessionFromMiddlewares(session);

            _logger.LogInformation($"The session disconnected: {session.SessionID} ({reason})");

            try
            {
                Interlocked.Decrement(ref _sessionCount);

                var closeEventArgs = new CloseEventArgs(reason);
                await session.FireSessionClosedAsync(closeEventArgs);
                await OnSessionClosedAsync(session, closeEventArgs);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "There is one exception thrown from the event of OnSessionClosed.");
            }
        }

        /// <summary>
        /// Handles the session connected event from the session event host interface.
        /// </summary>
        /// <param name="session">The connected session.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask ISessionEventHost.HandleSessionConnectedEvent(IAppSession session)
        {
            return FireSessionConnectedEvent((AppSession)session);
        }

        /// <summary>
        /// Handles the session closed event from the session event host interface.
        /// </summary>
        /// <param name="session">The closed session.</param>
        /// <param name="reason">The reason for the session closure.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask ISessionEventHost.HandleSessionClosedEvent(IAppSession session, CloseReason reason)
        {
            return FireSessionClosedEvent((AppSession)session, reason);
        }

        /// <summary>
        /// Handles a session by processing its incoming packages.
        /// </summary>
        /// <param name="session">The session to handle.</param>
        /// <param name="connection">The connection associated with the session.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async ValueTask HandleSession(AppSession session, IConnection connection)
        {
            if (!await InitializeSession(session, connection))
                return;

            try
            {
                var pipelineFilter = _pipelineFilterFactory.Create();
                pipelineFilter.Context = CreatePipelineContext(session);

                var packageStream = connection.RunAsync<TReceivePackageInfo>(pipelineFilter);

                await FireSessionConnectedEvent(session);

                var packageHandlingScheduler = _packageHandlingScheduler;

#if NET6_0_OR_GREATER
                using var cancellationTokenSource = GetPackageHandlingCancellationTokenSource(connection.ConnectionToken);
#endif
                ValueTask prevPackageHandlingTask = ValueTask.CompletedTask;

                await foreach (var p in packageStream)
                {
                    if (_packageHandlingContextAccessor != null)
                    {
                        _packageHandlingContextAccessor.PackageHandlingContext = new PackageHandlingContext<IAppSession, TReceivePackageInfo>(session, p);
                    }

#if !NET6_0_OR_GREATER
                    using var cancellationTokenSource = GetPackageHandlingCancellationTokenSource(connection.ConnectionToken);
#endif
                    if (prevPackageHandlingTask != ValueTask.CompletedTask)
                    {
                        await prevPackageHandlingTask;
                    }

                    prevPackageHandlingTask = packageHandlingScheduler.HandlePackage(session, p, cancellationTokenSource.Token);

#if NET6_0_OR_GREATER
                    cancellationTokenSource.TryReset();
#endif
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to handle the session {session.SessionID}.");
            }
        }

        /// <summary>
        /// Gets a cancellation token source for package handling with a timeout.
        /// </summary>
        /// <param name="cancellationToken">The base cancellation token to link with.</param>
        /// <returns>A new cancellation token source linked to the provided token with a timeout.</returns>
        protected virtual CancellationTokenSource GetPackageHandlingCancellationTokenSource(CancellationToken cancellationToken)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(Options.PackageHandlingTimeOut));
            return cancellationTokenSource;
        }

        /// <summary>
        /// Called when an error occurs while processing a package for a session.
        /// </summary>
        /// <param name="session">The session where the error occurred.</param>
        /// <param name="exception">The exception that was thrown during package handling.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the session should continue processing packages.</returns>
        protected virtual ValueTask<bool> OnSessionErrorAsync(IAppSession session, PackageHandlingException<TReceivePackageInfo> exception)
        {
            _logger.LogError(exception, $"Session[{session.SessionID}]: session exception.");
            return new ValueTask<bool>(true);
        }

        /// <summary>
        /// Starts the SuperSocket service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task<bool> StartAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.None && state != ServerState.Stopped && state != ServerState.Failed)
            {
                throw new InvalidOperationException($"The server cannot be started right now, because its state is {state}.");
            }

            _state = ServerState.Starting;

            foreach (var m in _middlewares)
            {
                m.Start(this);
            }

            if (!await StartListenAsync(cancellationToken))
            {
                _state = ServerState.Failed;
                _logger.LogError("Failed to start any listener.");
                return false;
            }

            _state = ServerState.Started;

            try
            {
                await OnStartedAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There is one exception thrown from the method OnStartedAsync().");
            }

            return true;
        }

        /// <summary>
        /// Called after the server has started successfully.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual ValueTask OnStartedAsync()
        {
#if NETSTANDARD2_1
            return GetCompletedTask();
#else
            return ValueTask.CompletedTask;
#endif
        }

        /// <summary>
        /// Called before the server stops.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual ValueTask OnStopAsync()
        {
#if NETSTANDARD2_1
            return GetCompletedTask();
#else
            return ValueTask.CompletedTask;
#endif
        }

        /// <summary>
        /// Stops a connection listener.
        /// </summary>
        /// <param name="listener">The listener to stop.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task StopListener(IConnectionListener listener)
        {
            await listener.StopAsync().ConfigureAwait(false);
            _logger.LogInformation($"The listener [{listener}] has been stopped.");
        }

        /// <summary>
        /// Stops the SuperSocket service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.Started)
            {
                throw new InvalidOperationException($"The server cannot be stopped right now, because its state is {state}.");
            }

            _state = ServerState.Stopping;

            var tasks = _connectionListeners.Where(l => l.IsRunning).Select(l => StopListener(l))
                .Union(new Task[] { Task.Run(ShutdownMiddlewares) });

            await Task.WhenAll(tasks).ConfigureAwait(false);

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

        /// <summary>
        /// Implementation of IHostedService.StartAsync. Starts the SuperSocket service and throws an exception if it fails.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            if (!await StartAsync(cancellationToken))
            {
                throw new Exception("Failed to start the server.");
            }
        }

        #region IDisposable Support
        /// <summary>
        /// Flag to detect redundant dispose calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Asynchronously disposes of resources used by the service.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync(true);

        /// <summary>
        /// Asynchronously releases the unmanaged resources used by the service and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

                    var connectionListeners = _connectionListeners;

                    if (connectionListeners != null && connectionListeners.Any())
                    {
                        foreach (var listener in connectionListeners)
                        {
                            listener.Dispose();
                        }
                    }
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the service and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            DisposeAsync(disposing).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Implementation of IDisposable.Dispose. Releases all resources used by the service.
        /// </summary>
        void IDisposable.Dispose()
        {
            DisposeAsync(true).GetAwaiter().GetResult();
        }

        #endregion
    }
}
