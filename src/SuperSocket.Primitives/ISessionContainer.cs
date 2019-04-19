using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionContainer
    {
        IAppSession GetSessionByID(string sessionID);
    }

    public interface IAsyncSessionContainer
    {
        Task<IAppSession> GetSessionByIDAsync(string sessionID);
    }
}