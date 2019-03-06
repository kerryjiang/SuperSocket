using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        
        IPackageDecoder<TPackageInfo> Decoder { get; set; }

        TPackageInfo Filter(ref SequenceReader<byte> reader);

        IPipelineFilter<TPackageInfo> NextFilter { get; }
    }
}