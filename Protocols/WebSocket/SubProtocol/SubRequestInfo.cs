using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// SubProtocol RequestInfo type
    /// </summary>
    public class SubRequestInfo : RequestInfo<string>, ISubRequestInfo
    {
        /// <summary>
        /// Gets the token of this request, used for callback
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubRequestInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="token">The token.</param>
        /// <param name="data">The data.</param>
        public SubRequestInfo(string key, string token, string data)
            : base(key, data)
        {
            Token = token;
        }
    }
}
