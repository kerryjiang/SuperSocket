using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public interface IRequestFilterFactory<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer);
    }
}
