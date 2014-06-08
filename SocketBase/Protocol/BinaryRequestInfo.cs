using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Binary type request information
    /// </summary>
    public class BinaryRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRequestInfo"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="body">The body.</param>
        public BinaryRequestInfo(string key, IList<ArraySegment<byte>> body)
        {
            Key = key;
            Body = body;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public IList<ArraySegment<byte>> Body { get; private set; }
    }
}
