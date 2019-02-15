using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public abstract class PipelineFilterBase<TPackageInfo> : IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        public IPipelineFilter<TPackageInfo> NextFilter { get; protected set; }

        public abstract TPackageInfo Filter(ref SequenceReader<byte>  reader);
    }
}