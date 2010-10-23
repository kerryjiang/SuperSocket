using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore.Configuration
{
    public class CertificateConfig : ConfigurationElement, ICertificateConfig
    {
        #region ICertificateConfig Members

        [ConfigurationProperty("certificateFilePath", IsRequired = true)]
        public string CertificateFilePath
        {
            get
            {
                return this["certificateFilePath"] as string;
            }
        }

        [ConfigurationProperty("certificatePassword", IsRequired = true)]
        public string CertificatePassword
        {
            get
            {
                return this["certificatePassword"] as string;
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
