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

        public override ValueTask<bool> HandleSession(IAppSession session)
        {
            session.Closed += OnSessionClosed;
            _sessions.TryAdd(session.SessionID, session);
            return new ValueTask<bool>(true);
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

        public IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> critera = null)
        {
            var enumerator = _sessions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var s = enumerator.Current.Value;

                if(critera == null || critera(s))
                    yield return s;
            }
        }

        public IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> critera = null) where TAppSession : IAppSession
        {
            var enumerator = _sessions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value is TAppSession s)
                {
                    if (critera == null || critera(s))
                        yield return s;
                }
            }
        }
    }
}
