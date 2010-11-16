using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Config
{
    public interface IProtocolConfig
    {
        string Name { get; }
        string Type { get; }
    }
}
