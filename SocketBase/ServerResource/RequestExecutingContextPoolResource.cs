using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.ServerResource
{
    class RequestExecutingContextPoolResource<TAppSession, TPackageInfo> : ServerResourceItem<IPool<RequestExecutingContext<TAppSession, TPackageInfo>>>
        where TPackageInfo : IPackageInfo
        where TAppSession : IAppSession, IThreadExecutingContext, IAppSession<TAppSession, TPackageInfo>, new()
    {
        public RequestExecutingContextPoolResource()
            : base("RequestExecutingContextPool")
        {

        }

        protected override IPool<RequestExecutingContext<TAppSession, TPackageInfo>> CreateResource(IServerConfig config)
        {
            IPool<RequestExecutingContext<TAppSession, TPackageInfo>> pool = null;
            pool = new IntelliPool<RequestExecutingContext<TAppSession, TPackageInfo>>(Math.Min(Math.Max(config.MaxConnectionNumber / 15, 100), config.MaxConnectionNumber), pool.CreateDefaultPoolItemCreator());
            return pool;
        }
    }
}
