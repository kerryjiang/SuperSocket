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

        private IServiceProvider _serviceProvider;

        private static ILogger _logger;

        private TaskCompletionSource<bool> _tcs;

        public ServerPushMiddleware(IServiceProvider serviceProvider, ILogger<ServerPushMiddleware> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private Timer _timer;

        private int _interval = 1;

        private int _total;

        private long _totalSecondsSpent = 0;
        private long _totalRounds = 0;
        private int _totalClients = 0;

        public override void Start(IServer server)
        {
            _sessionContainer = _serviceProvider.GetSessionContainer();
            _tcs = new TaskCompletionSource<bool>();
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
            if (_tcs != null)
            {
                await _tcs.Task;
                _tcs = null;
            }
            
            // about 300 characters
            var line = string.Join("-", Enumerable.Range(0, 10).Select(x => Guid.NewGuid().ToString()));

            var startTime = DateTime.Now;

            foreach (var s in _sessionContainer.GetSessions<PushSession>())
            {
                await s.SendAsync(line);
            }

            _totalSecondsSpent += DateTime.Now.Subtract(startTime).Seconds;
            _totalRounds += 1;

            _total += line.Length;
        }

        public void StartPush(int totalClients)
        {
            _totalClients = totalClients;
            _tcs.SetResult(true);
        }

        public override void Shutdown(IServer server)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
            var v = (float)_totalSecondsSpent/(float)_totalRounds;
            _logger.LogInformation($"Sent {_total} bytes to {_totalClients} clients with {_totalRounds} rounds at the speed {v} seconds/round.");
        }
    }
}
