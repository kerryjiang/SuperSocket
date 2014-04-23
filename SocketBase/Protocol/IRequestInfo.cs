using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Request information interface
    /// </summary>
    public interface IRequestInfo : IRequestInfo<string>
    {

    }

    /// <summary>
    /// Request information interface
    /// </summary>
    public interface IRequestInfo<out TKey> : IPackageInfo<TKey>
    {

    }
}
