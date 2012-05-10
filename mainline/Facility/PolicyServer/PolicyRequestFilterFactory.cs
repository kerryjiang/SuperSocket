using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    class PolicyRequestFilterFactory : IRequestFilterFactory<BinaryRequestInfo>
    {
        /// <summary>
        /// Gets the size of the fix request.
        /// </summary>
        /// <value>
        /// The size of the fix request.
        /// </value>
        public int FixRequestSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRequestFilterFactory"/> class.
        /// </summary>
        /// <param name="fixRequestSize">Size of the fix request.</param>
        public PolicyRequestFilterFactory(int fixRequestSize)
        {
            FixRequestSize = fixRequestSize;
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns></returns>
        public IRequestFilter<BinaryRequestInfo> CreateFilter(SocketBase.IAppServer appServer, SocketBase.ISocketSession socketSession)
        {
            return new PolicyRequestFilter(FixRequestSize);
        }
    }
}
