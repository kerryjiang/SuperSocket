using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;

namespace SuperSocket.SessionContainer
{
    public class InProcSessionContainerMiddleware : MiddlewareBase, ISessionContainer
    {
        private ConcurrentDictionary<string, IAppSession> _sessions;

        public InProcSessionContainerMiddleware(IServiceProvider serviceProvider)
        {
            _sessions = new ConcurrentDictionary<string, IAppSession>(StringComparer.OrdinalIgnoreCase);
        }

        public override void Register(IServer server, IAppSession session)
        {
            session.Closed += OnSessionClosed;
            _sessions.TryAdd(session.SessionID, session);
        }

        private void OnSessionClosed(object sender, EventArgs e)
        {
            var session  = (IAppSession)sender;

            session.Closed -= OnSessionClosed;
            _sessions.TryRemove(session.SessionID, out IAppSession removedSession);
        }

        public IAppSession GetSessionByID(string sessionID)
        {
            _sessions.TryGetValue(sessionID, out IAppSession session);
            return session;
        }

        public int GetSessionCount()
        {
            return _sessions.Count;
        }
    }
}
