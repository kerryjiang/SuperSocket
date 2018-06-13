using System;

namespace SuperSocket.ProtoBase
{
    public abstract class FilterResult
    {
        public ProcessState State { get; set; }
    }

    public class FilterResult<TPackageInfo> : FilterResult
    {
        public TPackageInfo Package { get; set; }

        public FilterResult(TPackageInfo package)
        {
            Package = package;
        }
    }
}