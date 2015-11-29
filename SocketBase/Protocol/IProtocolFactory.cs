using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// The interface for protocol factory
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IProtocolFactory<out TPackageInfo> : IReceiveFilterFactory<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {

    }
}
