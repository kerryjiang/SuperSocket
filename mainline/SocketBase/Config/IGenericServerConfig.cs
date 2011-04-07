using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace SuperSocket.SocketBase.Config
{
    public interface IGenericServerConfig
    {
        string Name { get; }

        string Type { get; }

        NameValueCollection Options { get; }
    }
}
