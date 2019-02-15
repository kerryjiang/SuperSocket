using System;
using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        TPackageInfo Filter(ref SequenceReader<byte> reader);

        IPipelineFilter<TPackageInfo> NextFilter { get; }
    }
}