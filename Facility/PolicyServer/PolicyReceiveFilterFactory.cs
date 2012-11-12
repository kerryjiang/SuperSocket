using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.Facility.PolicyServer
{
    class PolicyReceiveFilterFactory : IReceiveFilterFactory<BinaryRequestInfo>
    {
        /// <summary>
        /// Gets the size of the fix request.
        /// </summary>
        /// <value>
        /// The size of the fix request.
        /// </value>
        public int FixRequestSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyReceiveFilterFactory"/> class.
        /// </summary>
        /// <param name="fixRequestSize">Size of the fix request.</param>
        public PolicyReceiveFilterFactory(int fixRequestSize)
        {
            FixRequestSize = fixRequestSize;
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IReceiveFilter<BinaryRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new PolicyReceiveFilter(FixRequestSize);
        }
    }
}
