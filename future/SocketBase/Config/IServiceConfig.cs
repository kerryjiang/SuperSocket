using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace SuperSocket.SocketBase.Config
{
    public interface IServiceConfig
    {
        string ServiceName { get; }

        string Type { get; }

        NameValueConfigurationCollection Providers { get; }

        bool Disabled { get; }
    }
}
