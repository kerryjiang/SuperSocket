using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Type provider configuration
    /// </summary>
    public class TypeProvider : ConfigurationElement, ITypeProvider
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }
    }
}
