using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    public abstract class FilterResult
    {
        public ProcessState State { get; set; }
    }

    public class FilterResult<TPackageInfo> : FilterResult
    {
        public TPackageInfo Package { get; set; }
    }
}