using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class SyncToAsyncSessionContainerWrapper : IAsyncSessionContainer
    {
        ISessionContainer _syncSessionContainer;

        public ISessionContainer SessionContainer
        {
            get { return _syncSessionContainer; }
        }

        public SyncToAsyncSessionContainerWrapper(ISessionContainer syncSessionContainer)
        {
            _syncSessionContainer = syncSessionContainer;
        }

        public ValueTask<IAppSession> GetSessionByIDAsync(string sessionID)
        {
            return new ValueTask<IAppSession>(_syncSessionContainer.GetSessionByID(sessionID));
        }

        public ValueTask<int> GetSessionCountAsync()
        {
            return new ValueTask<int>(_syncSessionContainer.GetSessionCount());
        }

        public ValueTask<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> criteria = null)
        {
            return new ValueTask<IEnumerable<IAppSession>>(_syncSessionContainer.GetSessions(criteria));
        }

        public ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> criteria = null) where TAppSession : IAppSession
        {
            return new ValueTask<IEnumerable<TAppSession>>(_syncSessionContainer.GetSessions<TAppSession>(criteria));
        }
    }
}