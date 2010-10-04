using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Config;
using System.Configuration;

namespace SuperSocket.SocketServiceCore.Configuration
{
    public class CredentialConfig : ConfigurationElement, ICredentialConfig
    {
        #region ICredentialConfig Members

        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get { return this["userName"] as string; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return this["password"] as string; }
        }

        #endregion
    }
}
