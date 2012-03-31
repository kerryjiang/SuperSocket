using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    /// <summary>
    /// FixSizeRequestFilterFactory
    /// </summary>
    public class FixSizeRequestFilterFactory : IRequestFilterFactory<BinaryRequestInfo>
    {
        /// <summary>
        /// Gets the size of the fix request.
        /// </summary>
        /// <value>
        /// The size of the fix request.
        /// </value>
        public int FixRequestSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixSizeRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="fixRequestSize">Size of the fix request.</param>
        public FixSizeRequestFilterFactory(int fixRequestSize)
        {
            FixRequestSize = fixRequestSize;
        }

        /// <summary>
        /// Creates the request filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns>
        /// the new created request filer assosiated with this socketSession
        /// </returns>
        public IRequestFilter<BinaryRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new FixSizeRequestFilter(FixRequestSize);
        }
    }
}
