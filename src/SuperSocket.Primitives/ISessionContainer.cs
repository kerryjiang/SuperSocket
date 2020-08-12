using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionContainer
    {
        IAppSession GetSessionByID(string sessionID);

        int GetSessionCount();

        IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria = null);

        IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria = null)
            where TAppSession : IAppSession;
    }

    public interface IAsyncSessionContainer
    {
        ValueTask<IAppSession> GetSessionByIDAsync(string sessionID);

        ValueTask<int> GetSessionCountAsync();

        ValueTask<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> criteria = null);

        ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> criteria = null)
            where TAppSession : IAppSession;
    }
}