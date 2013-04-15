using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// The basic interface of SubRequestInfo
    /// </summary>
    public interface ISubRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        string Token { get; }
    }
}
