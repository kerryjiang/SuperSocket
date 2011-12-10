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

        public string FilePath { get; set; }

        public string Password { get; set; }

        public string StoreName { get; set; }
        
        public string Thumbprint { get; set; }

        #endregion
    }
}
