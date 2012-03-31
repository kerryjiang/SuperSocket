using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Service configuration
    /// </summary>
    public class Service : ConfigurationElementBase, IServiceConfig
    {
        #region IServiceConfig Members
        /// <summary>
        /// Gets the type.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IServiceConfig"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("disabled", DefaultValue = "false")]
        public bool Disabled
        {
            get { return (bool)this["disabled"]; }
        }
        #endregion
    }
}
