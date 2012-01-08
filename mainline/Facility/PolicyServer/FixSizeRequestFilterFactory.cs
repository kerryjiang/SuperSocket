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

        public IRequestFilter<BinaryRequestInfo> CreateFilter(IAppServer appServer)
        {
            return new FixSizeRequestFilter(FixRequestSize);
        }
    }
}
