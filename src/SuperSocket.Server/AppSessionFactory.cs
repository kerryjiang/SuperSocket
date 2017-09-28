using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    class AppSessionFactory<TPackageInfo, TPipelineFilter> : IAppSessionFactory
        where TPackageInfo : class
        where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
    {
        Action<IAppSession, TPackageInfo> _packageHandler;
        
        public AppSessionFactory(Action<IAppSession, TPackageInfo> handler)
        {
            _packageHandler = handler;
        }

        public IAppSession Create(IPipeConnection pipeConnection)
        {
            var session = new AppSession<TPackageInfo>(new TPipelineFilter());
            session.Initialize(pipeConnection);
            session.PackageReceived += _packageHandler;
            return session;
        }
    }
}