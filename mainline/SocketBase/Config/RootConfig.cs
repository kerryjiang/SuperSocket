using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public class RootConfig : IRootConfig
    {
        #region IRootConfig Members

        public ICredentialConfig CredentialConfig { get; set; }

        public string ConsoleBaseAddress { get; set; }

        public LoggingMode LoggingMode { get; set; }

        [Obsolete]
        public bool IndependentLogger { get; set; }

        #endregion
    }
}
