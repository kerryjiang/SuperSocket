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

        [ConfigurationProperty("filePath", IsRequired = false)]
        public string FilePath
        {
            get
            {
                return this["filePath"] as string;
            }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return this["password"] as string;
            }
        }

        [ConfigurationProperty("storeName", IsRequired = false)]
        public string StoreName
        {
            get
            {
                return this["storeName"] as string;
            }
        }

        [ConfigurationProperty("thumbprint", IsRequired = false)]
        public string Thumbprint
        {
            get
            {
                return this["thumbprint"] as string;
            }
        }

        [ConfigurationProperty("isEnabled", IsRequired = false, DefaultValue = true)]
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
