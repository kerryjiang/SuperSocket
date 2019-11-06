using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter
    {
        void Reset();

        object Context { get; set; }        
    }

    public interface IPipelineFilter<TPackageInfo> : IPipelineFilter
        where TPackageInfo : class
    {
        
        IPackageDecoder<TPackageInfo> Decoder { get; set; }

        TPackageInfo Filter(ref SequenceReader<byte> reader);

        IPipelineFilter<TPackageInfo> NextFilter { get; }
        
    }
}