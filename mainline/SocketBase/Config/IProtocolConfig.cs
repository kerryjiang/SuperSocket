using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IProtocolConfig
    {
        string Name { get; }
        string Type { get; }
    }
}
