using System.Buffers;
using System.IO.Pipelines;

namespace SuperSocket.ProtoBase
{
    public interface IPackageEncoder<in TPackageInfo>
    {
        int Encode(PipeWriter writer, TPackageInfo pack);
    }
}