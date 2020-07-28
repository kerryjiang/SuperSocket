using Super.Engine;
using SuperSocket;
using System;

namespace Super.Host
{
    public class OnlineSessionFactory : ISessionFactory
    {
        private readonly IServiceProvider _serviceProvider;
     
        public Type SessionType => typeof(IOnlineSession);
      
        public OnlineSessionFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    
        public IAppSession Create() => _serviceProvider.GetService(SessionType).As<IOnlineSession>();
    }
}
