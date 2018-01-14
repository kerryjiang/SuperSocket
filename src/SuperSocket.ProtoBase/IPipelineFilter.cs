using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        TPackageInfo Filter(ref ReadOnlyBuffer<byte> buffer);

        IPipelineFilter<TPackageInfo> NextFilter { get; }
    }
}