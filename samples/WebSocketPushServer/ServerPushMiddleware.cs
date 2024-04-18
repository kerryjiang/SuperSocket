using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SuperSocket;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Server.Abstractions.Middleware;
using SuperSocket.WebSocket.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace WebSocketPushServer
{
    class ServerPushMiddleware : MiddlewareBase
    {
        private ISessionContainer _sessionContainer;

        private static ILogger _logger;

        private Task _sendTask;

        private bool _stopped = false;

        public ServerPushMiddleware(ILogger<ServerPushMiddleware> logger)
        {
            _logger = logger;
        }

        public override void Start(IServer server)
        {
            _sessionContainer = server.GetSessionContainer();
            _sendTask = RunAsync();
        }

        private async Task RunAsync()
        {
            while (!_stopped)
            {
                var sent = await Push();

                if (sent == 0 && !_stopped)
                {
                    await Task.Delay(1000 * 5);
                }
            }
        }

        private async ValueTask<int> Push()
        {
            // about 300 characters
            var line = string.Join("-", Enumerable.Range(0, 10).Select(x => Guid.NewGuid().ToString()));
            var startTime = DateTime.Now;

            var count = 0;

            foreach (var s in _sessionContainer.GetSessions<PushSession>())
            {
                await s.SendAsync(line, CancellationToken.None);
                count++;

                if (_stopped)
                    break;
            }

            return count;
        }

        public override void Shutdown(IServer server)
        {
            _stopped = true;

            _sendTask.Wait();

            foreach (var s in _sessionContainer.GetSessions<PushSession>())
            {
                s.PrintStats();
            }
        }
    }
}
