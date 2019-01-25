using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace SuperSocket.Server
{
    internal class ConnectionDispatcher<TPackageInfo, TPipelineFilter> : IConnectionDispatcher
        where TPackageInfo : class
        where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
    {
        public Task OnConnection(TransportConnection connection)
        {
            var session = new AppSession<TPackageInfo>(connection, new TPipelineFilter());
            return session.ProcessRequest();
        }
    }
}