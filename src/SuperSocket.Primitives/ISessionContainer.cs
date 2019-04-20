using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionContainer
    {
        IAppSession GetSessionByID(string sessionID);

        int GetSessionCount();
    }

    public interface IAsyncSessionContainer
    {
        Task<IAppSession> GetSessionByIDAsync(string sessionID);

        Task<int> GetSessionCountAsync();
    }
}