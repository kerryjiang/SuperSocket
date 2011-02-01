using System;
using System.Configuration;
using System.Collections.Specialized;

namespace SuperSocket.SocketBase.Config
{
    public interface IConnectionFilterConfig
    {
        string Name { get; }

        string Type { get; }

        NameValueCollection Options { get; }
    }
}

