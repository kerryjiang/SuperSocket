using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    public class CertificateConfig : ConfigurationElement, ICertificateConfig
    {
        #region ICertificateConfig Members

        [ConfigurationProperty("filePath", IsRequired = true)]
        public string FilePath
        {
            get
            {
                return this["filePath"] as string;
            }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return this["password"] as string;
            }
        }

        [ConfigurationProperty("isEnabled", IsRequired = false, DefaultValue = false)]
        public bool IsEnabled
        {
            get
            {
                return (bool)this["isEnabled"];
            }
        }

        #endregion ICertificateConfig Members
    }
}
