using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    class DefaultSessionFactory : ISessionFactory
    {
        public Type SessionType => typeof(AppSession);

        public IAppSession Create()
        {
            return new AppSession();
        }
    }
}