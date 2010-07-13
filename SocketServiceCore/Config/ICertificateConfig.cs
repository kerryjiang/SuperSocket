using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore.Config
{
    public interface ICertificateConfig
    {
        string CertificateFilePath { get; }

        string CertificatePassword { get; }
    }
}
