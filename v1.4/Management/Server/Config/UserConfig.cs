using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using System.Configuration;

namespace SuperSocket.Management.Server.Config
{
    public class UserConfig : ConfigurationElementBase
    {
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
