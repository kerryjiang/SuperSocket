using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using System.Configuration;

namespace SuperSocket.ServerManager.Config
{
    /// <summary>
    /// User configuration
    /// </summary>
    public class UserConfig : ConfigurationElementBase
    {
        /// <summary>
        /// Gets the password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
        }
    }
}
