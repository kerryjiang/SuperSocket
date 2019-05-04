using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class GenericSessionFactory<TSession> : ISessionFactory
        where TSession : AppSession, new()
    {
        public IAppSession Create()
        {
            return new TSession();
        }
    }
}