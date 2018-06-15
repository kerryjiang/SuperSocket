using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;

namespace SuperSocket.Server
{
    public class AppSession<TPackageInfo> : PipeChannel<TPackageInfo>, IAppSession
        where TPackageInfo : class
    {
        public AppSession(TransportConnection transportConnection, IPipelineFilter<TPackageInfo> pipelineFilter)
            : base(transportConnection, pipelineFilter)
        {

        }

        public AppServer AppServer { get; internal set; }
    }
}