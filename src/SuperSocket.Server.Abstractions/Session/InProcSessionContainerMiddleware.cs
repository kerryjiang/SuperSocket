using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Middleware;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// A middleware that implements the session container in the current process memory.
    /// </summary>
    public class InProcSessionContainerMiddleware : MiddlewareBase, ISessionContainer
    {
        private ConcurrentDictionary<string, IAppSession> _sessions;

        /// <summary>
        /// Initializes a new instance of the <see cref="InProcSessionContainerMiddleware"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public InProcSessionContainerMiddleware(IServiceProvider serviceProvider)
        {
            Order = int.MaxValue; // make sure it is the last middleware
            _sessions = new ConcurrentDictionary<string, IAppSession>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a session in the container.
        /// </summary>
        /// <param name="session">The session to register.</param>
        /// <returns>A task representing the asynchronous operation with a boolean result indicating whether the registration was successful.</returns>
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

        /// <summary>
        /// Unregisters a session from the container.
        /// </summary>
        /// <param name="session">The session to unregister.</param>
        /// <returns>A task representing the asynchronous operation with a boolean result indicating whether the unregistration was successful.</returns>
        public override ValueTask<bool> UnRegisterSession(IAppSession session)
        {
            _sessions.TryRemove(session.SessionID, out IAppSession removedSession);
            return new ValueTask<bool>(true);
        }

        /// <summary>
        /// Gets a session by its unique identifier.
        /// </summary>
        /// <param name="sessionID">The unique identifier of the session.</param>
        /// <returns>The session if found; otherwise, null.</returns>
        public IAppSession GetSessionByID(string sessionID)
        {
            _sessions.TryGetValue(sessionID, out IAppSession session);
            return session;
        }

        /// <summary>
        /// Gets the total count of sessions in the container.
        /// </summary>
        /// <returns>The number of sessions.</returns>
        public int GetSessionCount()
        {
            return _sessions.Count;
        }

        /// <summary>
        /// Gets sessions that match the specified criteria.
        /// </summary>
        /// <param name="criteria">The predicate used to filter sessions, or null to get all sessions.</param>
        /// <returns>An enumerable collection of sessions that match the criteria.</returns>
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

        /// <summary>
        /// Gets sessions of a specific type that match the specified criteria.
        /// </summary>
        /// <typeparam name="TAppSession">The type of sessions to retrieve.</typeparam>
        /// <param name="criteria">The predicate used to filter sessions, or null to get all sessions of the specified type.</param>
        /// <returns>An enumerable collection of sessions that match the criteria.</returns>
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
