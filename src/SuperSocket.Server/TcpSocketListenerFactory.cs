using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Buffers;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public class TcpSocketListenerFactory : IListenerFactory
    {
        public IListener CreateListener<TPackageInfo>(ListenOptions options, object pipelineFilterFactory)
            where TPackageInfo : class
        {
            var filterFactory = pipelineFilterFactory as IPipelineFilterFactory<TPackageInfo>;
            return new TcpSocketListener(options, (s) => new TcpPipeChannel<TPackageInfo>(s, filterFactory.Create(s)));
        }
    }
}