using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// IRootConfig, the part compatible with .Net 4.5 or higher
    /// </summary>
    public partial interface IRootConfig
    {
        /// <summary>
        /// Gets the default culture for all server instances.
        /// </summary>
        /// <value>
        /// The default culture.
        /// </value>
        string DefaultCulture { get; }
    }
}
