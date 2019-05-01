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
    public class SuperSocketService<TReceivePackageInfo, TPipelineFilterFactory> : SuperSocketService<TReceivePackageInfo>
        where TReceivePackageInfo : class
        where TPipelineFilterFactory : IPipelineFilterFactory<TReceivePackageInfo>, new()
    {
        public SuperSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, ILoggerFactory loggerFactory, IChannelCreatorFactory channelCreatorFactory)
            : base(serviceProvider, serverOptions, loggerFactory, channelCreatorFactory)
        {

        }

        protected override IPipelineFilterFactory<TReceivePackageInfo> GetPipelineFilterFactory()
        {
            return new TPipelineFilterFactory();
        }
    }

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
        private Func<IAppSession, TReceivePackageInfo, Task> _packageHandler;
        private int _sessionCount;
        public string Name { get; }
        public int SessionCount => _sessionCount;

        private IMiddleware[] _middlewares;

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
            _packageHandler = serviceProvider.GetService<Func<IAppSession, TReceivePackageInfo, Task>>();

            InitializeMiddlewares();
        }

        private void InitializeMiddlewares()
        {
            _middlewares = _serviceProvider.GetServices<IMiddleware>().ToArray();
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

        private Task<bool> StartListenAsync()
        {
            _channelCreators = new List<IChannelCreator>();

            var serverOptions = _serverOptions.Value;

            if (serverOptions.Listeners != null && serverOptions.Listeners.Any())
            {
                foreach (var l in serverOptions.Listeners)
                {
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
            var session = new AppSession(this, channel);
            InitializeSession(session);            
            HandleSession(session).DoNotAwait();
        }

        void IChannelRegister.RegisterChannel(object connection)
        {
            var channel = _channelCreators.FirstOrDefault().CreateChannel(connection);
            AcceptNewChannel(channel);
        }

        private void InitializeSession(AppSession session)
        {
            var middlewares = _middlewares;

            if (middlewares != null && middlewares.Length > 0)
            {
                for (var i = 0; i < middlewares.Length; i++)
                {
                    middlewares[i].Register(this, session);
                }
            }

            var packageHandler = _packageHandler;

            if (packageHandler != null)
            {
                if (session.Channel is IChannel<TReceivePackageInfo> channel)
                {
                    channel.PackageReceived += async (ch, p) =>
                    {
                        try
                        {
                            await packageHandler(session, p);
                        }
                        catch (Exception e)
                        {
                            OnSessionError(session, e);
                        }
                    };
                }
            }
        }

        private async Task HandleSession(AppSession session)
        {
            Interlocked.Increment(ref _sessionCount);

            try
            {
                _logger.LogInformation($"A new session connected: {session.SessionID}");
                session.OnSessionConnected();
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartListenAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var tasks = _channelCreators.Where(l => l.IsRunning).Select(l => l.StopAsync()).ToArray();
            await Task.WhenAll(tasks);
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
    }
}
