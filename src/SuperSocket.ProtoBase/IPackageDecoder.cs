using System.Buffers;

namespace SuperSocket.ProtoBase
{
    public interface IPackageDecoder<out TPackageInfo>
        where TPackageInfo : class
    {
        TPackageInfo Decode(ReadOnlySequence<byte> buffer, object context);
    }
}