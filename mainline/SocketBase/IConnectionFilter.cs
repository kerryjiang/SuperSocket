using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Net;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase
{
    public interface IConnectionFilter
    {
        bool Initialize(string name, NameValueCollection options);
        
        string Name { get; }

        bool AllowConnect(IPEndPoint remoteAddress);
    }
}

