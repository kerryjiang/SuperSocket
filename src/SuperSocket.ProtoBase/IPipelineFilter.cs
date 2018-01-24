using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        TPackageInfo Filter(ref ReadOnlyBuffer<byte> buffer);

        IPipelineFilter<TPackageInfo> NextFilter { get; }
    }
}