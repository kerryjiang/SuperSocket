using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.ServerManager.Model
{
    /// <summary>
    /// Login command result
    /// </summary>
    public class LoginResult : CommandResult
    {
        /// <summary>
        /// Gets or sets the server metadata source.
        /// </summary>
        /// <value>
        /// The server metadata source.
        /// </value>
        public List<KeyValuePair<string, StatusInfoAttribute[]>> ServerMetadataSource { get; set; }
    }
}
