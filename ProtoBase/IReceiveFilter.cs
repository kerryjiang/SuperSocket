using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IReceiveFilter<out TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        TPackageInfo Filter(ReceiveCache data, out int rest);

        IReceiveFilter<TPackageInfo> NextReceiveFilter { get; }

        FilterState State { get; }

        void Reset();
    }
}
