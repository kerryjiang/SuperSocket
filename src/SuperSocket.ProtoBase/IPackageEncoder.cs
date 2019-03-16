using System.Buffers;
using System.IO.Pipelines;

namespace SuperSocket.ProtoBase
{
    public interface IPackageEncoder<in TPackageInfo>
        where TPackageInfo : class
    {
        int Encode(PipeWriter writer, TPackageInfo pack);
    }
}