using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface IRootConfig
    {
        ICredentialConfig CredentialConfig { get; }

        string ConsoleBaseAddress { get; }

        [Obsolete]
        bool IndependentLogger { get; }

        LoggingMode LoggingMode { get; }
    }
}
