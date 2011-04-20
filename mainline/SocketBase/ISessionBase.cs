using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SuperSocket.SocketBase
{
    public interface ISessionBase
    {
        /// <summary>
        /// Gets the session ID.
        /// </summary>
        string SessionID { get; }

        /// <summary>
        /// Gets the identity key.
        /// In most case, IdentityKey is same as SessionID
        /// </summary>
        string IdentityKey { get; }

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }
    }
}
