using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class AppSession<TPackageInfo> : PipeChannel<TPackageInfo>, IPipeChannel<TPackageInfo>, IPipeChannel, IAppSession
        where TPackageInfo : class
    {
        public AppSession(IPipelineFilter<TPackageInfo> pipelineFilter)
            : base(pipelineFilter)
        {

        }

        public AppServer AppServer { get; internal set; }
    }
}