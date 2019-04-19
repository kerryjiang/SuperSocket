using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class AsyncToSyncSessionContainerWraper : ISessionContainer
    {
        IAsyncSessionContainer _asyncSessionContainer;

        public AsyncToSyncSessionContainerWraper(IAsyncSessionContainer asyncSessionContainer)
        {
            _asyncSessionContainer = asyncSessionContainer;
        }

        public IAppSession GetSessionByID(string sessionID)
        {
            return _asyncSessionContainer.GetSessionByIDAsync(sessionID).Result;
        }
    }
}