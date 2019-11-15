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
    public class SuperSocketService<TReceivePackageInfo> : IHostedService, IServer, IChannelRegister
        where TReceivePackageInfo : class
    {
        private readonly IServiceProvider _serviceProvider;

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        private readonly IOptions<ServerOptions> _serverOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private IPipelineFilterFactory<TReceivePackageInfo> _pipelineFilterFactory;
        private IChannelCreatorFactory _channelCreatorFactory;
        private List<IChannelCreator> _channelCreators;
        private IPackageHandler<TReceivePackageInfo> _packageHandler;
        private Func<IAppSession, PackageHandlingException<TReceivePackageInfo>, ValueTask<bool>> _errorHandler;
        
        public string Name { get; }

        private int _sessionCount;

        public int SessionCount => _sessionCount;

        private ISessionFactory _sessionFactory;

        private IMiddleware[] _middlewares;

        private ServerState _state = ServerState.None;

        public ServerState State
        {
            get { return _state; }
        }

        public object DataContext { get; set; }

        public SuperSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, ILoggerFactory loggerFactory, IChannelCreatorFactory channelCreatorFactory)
        {
            _serverOptions = serverOptions;
            Name = serverOptions.Value.Name;
            _serviceProvider = serviceProvider;
            _pipelineFilterFactory = GetPipelineFilterFactory();
            _serverOptions = serverOptions;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger("SuperSocketService");
            _channelCreatorFactory = channelCreatorFactory;
            _packageHandler = serviceProvider.GetService<IPackageHandler<TReceivePackageInfo>>();
            _errorHandler = serviceProvider.GetService<Func<IAppSession, PackageHandlingException<TReceivePackageInfo>, ValueTask<bool>>>();

            if (_errorHandler == null)
            {
                _errorHandler = async (s, e) =>
                {
                    _logger.LogError(e.Message, e.InnerException);
                    return await new ValueTask<bool>(true);
                };
            }
            
            // initialize session factory
            _sessionFactory = serviceProvider.GetService<ISessionFactory>();

            if (_sessionFactory == null)
                _sessionFactory = new DefaultSessionFactory();


            InitializeMiddlewares();
        }

        private void InitializeMiddlewares()
        {
            _middlewares = _serviceProvider.GetServices<IMiddleware>().ToArray();

            foreach (var m in _middlewares)
            {
                m.Register(this);
            }

            if (_packageHandler == null)
                _packageHandler = _middlewares.OfType<IPackageHandler<TReceivePackageInfo>>().FirstOrDefault();
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
                _logger.LogWarning("No listner was defined, so this server only can accept connections from the ActiveConnect.");

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
                    var result = await middlewares[i].HandleSession(session);

                    if (!result)
                        return false;
                }
            }

            return true;
        }

        private async ValueTask HandleSession(AppSession session, IChannel channel)
        {
            var result = await InitializeSession(session, channel);

            if (!result)
                return;

            try
            {
                Interlocked.Increment(ref _sessionCount);

                _logger.LogInformation($"A new session connected: {session.SessionID}");
                session.OnSessionConnected();

                var packageChannel = channel as IChannel<TReceivePackageInfo>;

                await foreach (var p in packageChannel.RunAsync())
                {
                    try
                    {
                        await _packageHandler?.Handle(session, p);
                    }
                    catch (Exception e)
                    {
                        var toClose = await _errorHandler(session, new PackageHandlingException<TReceivePackageInfo>($"Session {session.SessionID} got an error when handle a package.", p, e));

                        if (toClose)
                        {
                            session.Close();
                        }
                    }                    
                }

                _logger.LogInformation($"The session disconnected: {session.SessionID}");

                session.OnSessionClosed(EventArgs.Empty);                
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to handle the session {session.SessionID}.", e);
            }
            finally
            {
                Interlocked.Decrement(ref _sessionCount);
            }
        }

        protected virtual void OnSessionError(IAppSession session, Exception exception)
        {
            _logger.LogError($"Session[{session.SessionID}]: session exception.", exception);
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
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var state = _state;

            if (state != ServerState.Started)
            {
                throw new InvalidOperationException($"The server cannot be stopped right now, because its state is {state}.");
            }

            _state = ServerState.Stopping;

            var tasks = _channelCreators.Where(l => l.IsRunning).Select(l => l.StopAsync()).ToArray();
            await Task.WhenAll(tasks);

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
                        if (_state != ServerState.Started)
                        {
                            await StopAsync(CancellationToken.None);
                        }
                    }
                    catch
                    {
                    }
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            DisposeAsync(true).GetAwaiter().GetResult();
        }

        #endregion
    }
}
