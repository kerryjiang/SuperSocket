using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineFilter<TPackageInfo>
        where TPackageInfo : class
    {
        FilterResult<TPackageInfo> Filter(ReadableBuffer buffer, out ReadCursor consumed, out ReadCursor examined);

        IPipelineFilter<TPackageInfo> NextFilter { get; }
    }
}