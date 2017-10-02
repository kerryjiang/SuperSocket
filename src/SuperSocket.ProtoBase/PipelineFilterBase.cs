using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public abstract class PipelineFilterBase<TPackageInfo> : IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }

        public abstract TPackageInfo Filter(ref ReadableBuffer buffer);
    }
}