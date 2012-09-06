using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Binary type request information
    /// </summary>
    public class BinaryRequestInfo :  RequestInfo<byte[]>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRequestInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        public BinaryRequestInfo(string key, byte[] body)
            : base(key, body)
        {

        }
    }
}
