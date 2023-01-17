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
            Order = int.MaxValue; // make sure it is the last middleware
            _sessions = new ConcurrentDictionary<string, IAppSession>(StringComparer.OrdinalIgnoreCase);
        }

        public override ValueTask<bool> RegisterSession(IAppSession session)
        {
            if (session is IHandshakeRequiredSession handshakeSession)
            {
                if (!handshakeSession.Handshaked)
                    return new ValueTask<bool>(true);
            }
            
            _sessions.TryAdd(session.SessionID, session);
            return new ValueTask<bool>(true);
        }

        public override ValueTask<bool> UnRegisterSession(IAppSession session)
        {
            _sessions.TryRemove(session.SessionID, out IAppSession removedSession);
            return new ValueTask<bool>(true);
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

        public IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria = null)
        {
            var enumerator = _sessions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var s = enumerator.Current.Value;

                if (s.State != SessionState.Connected)
                    continue;

                if(criteria == null || criteria(s))
                    yield return s;
            }
        }

        public IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria = null) where TAppSession : IAppSession
        {
            var enumerator = _sessions.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value is TAppSession s)
                {
                    if (s.State != SessionState.Connected)
                        continue;
                        
                    if (criteria == null || criteria(s))
                        yield return s;
                }
            }
        }
    }
}
