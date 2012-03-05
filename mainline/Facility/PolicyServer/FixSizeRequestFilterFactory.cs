using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    public class FixSizeRequestFilterFactory : IRequestFilterFactory<BinaryRequestInfo>
    {
        public int FixRequestSize { get; private set; }

        public FixSizeRequestFilterFactory(int fixRequestSize)
        {
            FixRequestSize = fixRequestSize;
        }

        /// <summary>
        /// Creates the request filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns>the new created request filer assosiated with this socketSession</returns>
        public IRequestFilter<BinaryRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new FixSizeRequestFilter(FixRequestSize);
        }
    }
}
