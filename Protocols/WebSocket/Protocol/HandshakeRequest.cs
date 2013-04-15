using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// Handshake request
    /// </summary>
    class HandshakeRequest : IWebSocketFragment
    {
        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key
        {
            get { return OpCode.HandshakeTag; }
        }
    }
}
