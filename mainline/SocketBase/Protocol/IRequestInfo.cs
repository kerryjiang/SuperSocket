using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public interface IRequestInfo
    {
        string Key { get; }
    }

    public interface IRequestInfo<TRequestData> : IRequestInfo
    {
        TRequestData Data { get; }
    }
}
