using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase
{
    class DictionarySessionContainer<TAppSession> : ISessionContainer<TAppSession>
        where TAppSession : IAppSession
    {
        private ConcurrentDictionary<string, TAppSession> m_Dict;

        public bool Initialize(IServerConfig config)
        {
            m_Dict = new ConcurrentDictionary<string, TAppSession>(4 * Environment.ProcessorCount, config.MaxConnectionNumber, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        public TAppSession GetSessionByID(string sessionID)
        {
            var session = default(TAppSession);
            m_Dict.TryGetValue(sessionID, out session);
            return session;
        }

        public bool TryRegisterSession(TAppSession session)
        {
            return m_Dict.TryAdd(session.SessionID, session);
        }

        public bool TryUnregisterSession(string sessionID)
        {
            TAppSession session;
            return m_Dict.TryRemove(sessionID, out session);
        }

        public IEnumerator<TAppSession> GetEnumerator()
        {
            return m_Dict.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return m_Dict.Count; }
        }
    }
}
