using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// UdpRequestInfo, it is designed for passing in business session ID to udp request info
    /// </summary>
    public class UdpRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpRequestInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="sessionID">The session ID.</param>
        public UdpRequestInfo(string key, string sessionID)
        {
            Key = key;
            SessionID = sessionID;
        }

        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public string SessionID { get; private set; }
    }
}
