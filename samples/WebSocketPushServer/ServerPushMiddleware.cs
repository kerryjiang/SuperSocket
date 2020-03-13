using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SuperSocket;
using SuperSocket.Server;
using SuperSocket.WebSocket.Server;
using SuperSocket.SessionContainer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace WebSocketPushServer
{
    class ServerPushMiddleware : MiddlewareBase
    {
        private ISessionContainer _sessionContainer;

        private ILogger<ServerPushMiddleware> _logger;

        public ServerPushMiddleware(IServiceProvider serviceProvider, ILogger<ServerPushMiddleware> logger)
        {
            _sessionContainer = serviceProvider.GetSessionContainer();
            _logger = logger;
        }

        private Timer _timer;

        private int _interval = 60;

        public override void Start(IServer server)
        {
            _timer = new Timer(OnTimerCallback, null, 1000 * _interval, 1000 * _interval);
        }

        private void OnTimerCallback(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                Push().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception happened in Push().");
            }

            _timer.Change(1000 * _interval, 1000 * _interval);
        }

        private async ValueTask Push()
        {
            // about 300 characters
            var line = string.Join("-", Enumerable.Range(0, 10).Select(x => Guid.NewGuid().ToString()));

            foreach (var s in _sessionContainer.GetSessions<PushSession>())
            {
                await s.SendAsync(line);
                s.Total = s.Total += line.Length;
            }
        }

        public override void Shutdown(IServer server)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }
    }
}
