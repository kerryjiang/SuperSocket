using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IPackageResolver<out TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);
    }
}
