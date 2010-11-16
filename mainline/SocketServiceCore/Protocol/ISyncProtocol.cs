using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public interface ISyncProtocol
    {
        ICommandStreamReader CreateSyncCommandReader();
    }
}
