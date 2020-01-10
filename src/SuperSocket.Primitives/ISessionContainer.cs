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

        IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> critera);

        IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> critera)
            where TAppSession : IAppSession;
    }

    public interface IAsyncSessionContainer
    {
        Task<IAppSession> GetSessionByIDAsync(string sessionID);

        Task<int> GetSessionCountAsync();

        Task<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> critera);

        Task<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> critera)
            where TAppSession : IAppSession;
    }
}