using System;
using System.Configuration;

namespace SuperSocket.Common
{
    public class ConfigurationElementBase : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }
    }
}

