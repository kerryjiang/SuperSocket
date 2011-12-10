using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface ICertificateConfig
    {
        bool IsEnabled { get; }

        string FilePath { get; }

        string Password { get; }

        string StoreName { get; }

        string Thumbprint { get; }
    }
}
