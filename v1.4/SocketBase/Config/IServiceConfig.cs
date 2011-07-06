using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IServiceConfig
    {
        string Name { get; }

        string Type { get; }

        NameValueCollection Options { get; }

        bool Disabled { get; }
    }
}
