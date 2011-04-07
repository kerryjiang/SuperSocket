using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    public interface IGenericServer
    {
        bool Initialize(IGenericServerConfig config, ILogger logger);

        void Start();

        void Stop();
    }
}
