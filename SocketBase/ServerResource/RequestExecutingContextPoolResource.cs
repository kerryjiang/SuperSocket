using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.ServerResource
{
    class RequestExecutingContextPoolResource<TAppSession, TRequestInfo> : ServerResourceItem<IPool<RequestExecutingContext<TAppSession, TRequestInfo>>>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession, IThreadExecutingContext, IAppSession<TAppSession, TRequestInfo>, new()
    {
        public RequestExecutingContextPoolResource()
            : base("RequestExecutingContextPool")
        {

        }

        protected override IPool<RequestExecutingContext<TAppSession, TRequestInfo>> CreateResource(IServerConfig config)
        {
            IPool<RequestExecutingContext<TAppSession, TRequestInfo>> pool = null;
            pool = new IntelliPool<RequestExecutingContext<TAppSession, TRequestInfo>>(Math.Min(Math.Max(config.MaxConnectionNumber / 15, 100), config.MaxConnectionNumber), pool.CreateDefaultPoolItemCreator());
            return pool;
        }
    }
}
