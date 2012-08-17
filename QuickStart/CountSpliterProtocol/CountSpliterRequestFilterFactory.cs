using System;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CountSpliterProtocol
{
    public class CountSpliterRequestFilterFactory<TRequestFilter, TRequestInfo> : IRequestFilterFactory<TRequestInfo>
        where TRequestFilter : CountSpliterRequestFilter<TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        public IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            var config = appServer.Config;

            if(config.MaxRequestLength > config.ReceiveBufferSize)
                throw new Exception("ReceiveBufferSize cannot smaller than MaxRequestLength in this protocol.");

            return new TRequestFilter();
        }
    }

    public class CountSpliterRequestFilterFactory<TRequestFilter> : CountSpliterRequestFilterFactory<TRequestFilter, StringRequestInfo>
        where TRequestFilter : CountSpliterRequestFilter<StringRequestInfo>, new()
    {

    }
}
