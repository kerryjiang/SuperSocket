using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public class CertificateConfig : ICertificateConfig
    {
        #region ICertificateConfig Members

        public bool IsEnabled { get; set; }

        public string CertificateFilePath { get; set; }

        public string CertificatePassword { get; set; }

        #endregion
    }
}
