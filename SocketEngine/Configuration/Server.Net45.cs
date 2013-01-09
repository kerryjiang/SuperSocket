using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine.Configuration
{
    /// <summary>
    /// Server, the port which is compatible with .Net 4.5 or higher
    /// </summary>
    public partial class Server : IServerConfig
    {
        /// <summary>
        /// Gets/sets the default culture for this server.
        /// </summary>
        /// <value>
        /// The default culture.
        /// </value>
        [ConfigurationProperty("defaultCulture", IsRequired = false)]
        public string DefaultCulture
        {
            get
            {
                return (string)this["defaultCulture"];
            }
        }
    }
}
