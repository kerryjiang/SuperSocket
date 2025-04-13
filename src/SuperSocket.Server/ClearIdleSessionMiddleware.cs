using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Abstractions.Middleware;

namespace SuperSocket.Server
{
    /// <summary>
    /// Middleware for clearing idle sessions based on their last active time.
    /// </summary>
    class ClearIdleSessionMiddleware : MiddlewareBase
    {
        private ISessionContainer _sessionContainer;

        private Timer _timer;

        private ServerOptions _serverOptions;

        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearIdleSessionMiddleware"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <param name="serverOptions">The server options containing configuration values.</param>
        /// <param name="loggerFactory">The logger factory to create loggers.</param>
        /// <exception cref="Exception">Thrown if the required <see cref="ISessionContainer"/> middleware is not available.</exception>
        public ClearIdleSessionMiddleware(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, ILoggerFactory loggerFactory)
        {
            _sessionContainer = serviceProvider.GetService<ISessionContainer>();
            
            if (_sessionContainer == null)
                throw new Exception($"{nameof(ClearIdleSessionMiddleware)} needs a middleware of {nameof(ISessionContainer)}");

            _serverOptions = serverOptions.Value;
            _logger = loggerFactory.CreateLogger<ClearIdleSessionMiddleware>();
        }

        /// <summary>
        /// Starts the middleware and initializes the timer for clearing idle sessions.
        /// </summary>
        /// <param name="server">The server instance.</param>
        public override void Start(IServer server)
        {
            _timer = new Timer(OnTimerCallback, null, _serverOptions.ClearIdleSessionInterval * 1000, _serverOptions.ClearIdleSessionInterval * 1000);
        }

        private void OnTimerCallback(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                var timeoutTime = DateTimeOffset.Now.AddSeconds(0 - _serverOptions.IdleSessionTimeOut);

                foreach (var s in _sessionContainer.GetSessions())
                {
                    if (s.LastActiveTime <= timeoutTime)
                    {
                        try
                        {
                            s.Connection.CloseAsync(CloseReason.TimeOut);
                            _logger.LogWarning($"Close the idle session {s.SessionID}, it's LastActiveTime is {s.LastActiveTime}.");
                        }
                        catch (Exception exc)
                        {
                            _logger.LogError(exc, $"Error happened when close the session {s.SessionID} for inactive for a while.");
                        }                        
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error happened when clear idle session.");
            }

            _timer.Change(_serverOptions.ClearIdleSessionInterval * 1000, _serverOptions.ClearIdleSessionInterval * 1000);
        }

        /// <summary>
        /// Shuts down the middleware and disposes of the timer.
        /// </summary>
        /// <param name="server">The server instance.</param>
        public override void Shutdown(IServer server)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
            _timer = null;
        }
    }
}