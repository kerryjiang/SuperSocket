using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using System.Threading;

namespace SuperSocket.Server
{
    internal interface ISuperSocketConnectionDispatcher : IConnectionDispatcher
    {
        int SessionCount { get; }
    }

    internal class ConnectionDispatcher<TPackageInfo, TPipelineFilter> : ISuperSocketConnectionDispatcher
        where TPackageInfo : class
        where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
    {
        private int _sessionCount;

        public int SessionCount
        {
            get { return _sessionCount; }
        }

        public Task OnConnection(TransportConnection connection)
        {
            var session = new AppSession<TPackageInfo>(connection, new TPipelineFilter());

            Interlocked.Increment(ref _sessionCount);

            try
            {
                return session.ProcessRequest();
            }
            finally
            {
                Interlocked.Decrement(ref _sessionCount);
            }
        }
    }
}