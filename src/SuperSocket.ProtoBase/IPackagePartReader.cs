using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPackagePartReader<TPackageInfo>
    {
        bool Process(TPackageInfo package, ref SequenceReader<byte> reader, out IPackagePartReader<TPackageInfo> nextPartReader, out bool needMoreData);
    }
}