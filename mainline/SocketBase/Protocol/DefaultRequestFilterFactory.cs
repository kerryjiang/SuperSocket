using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public class DefaultRequestFilterFactory<TRequestFilter, TRequestInfo> : IRequestFilterFactory<TRequestInfo>
        where TRequestInfo : IRequestInfo
        where TRequestFilter : IRequestFilter<TRequestInfo>, new()
    {
        public IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer)
        {
            return new TRequestFilter();
        }
    }
}
