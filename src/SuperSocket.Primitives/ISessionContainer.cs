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

        IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> critera = null);

        IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> critera = null)
            where TAppSession : IAppSession;
    }

    public interface IAsyncSessionContainer
    {
        Task<IAppSession> GetSessionByIDAsync(string sessionID);

        Task<int> GetSessionCountAsync();

        Task<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> critera = null);

        Task<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> critera = null)
            where TAppSession : IAppSession;
    }
}