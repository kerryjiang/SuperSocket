using System;
using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// RequestFilterFactory for CountSpliterRequestFilter
    /// </summary>
    /// <typeparam name="TRequestFilter">The type of the request filter.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public class CountSpliterRequestFilterFactory<TRequestFilter, TRequestInfo> : IRequestFilterFactory<TRequestInfo>
        where TRequestFilter : CountSpliterRequestFilter<TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            var config = appServer.Config;

            if(config.MaxRequestLength > config.ReceiveBufferSize)
                throw new Exception("ReceiveBufferSize cannot smaller than MaxRequestLength in this protocol.");

            return new TRequestFilter();
        }
    }

    /// <summary>
    /// RequestFilterFactory for CountSpliterRequestFilter
    /// </summary>
    /// <typeparam name="TRequestFilter">The type of the request filter.</typeparam>
    public class CountSpliterRequestFilterFactory<TRequestFilter> : CountSpliterRequestFilterFactory<TRequestFilter, StringRequestInfo>
        where TRequestFilter : CountSpliterRequestFilter<StringRequestInfo>, new()
    {

    }
}
