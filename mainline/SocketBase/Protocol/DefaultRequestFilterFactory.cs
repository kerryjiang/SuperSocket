using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// DefaultRequestFilterFactory
    /// </summary>
    /// <typeparam name="TRequestFilter">The type of the request filter.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public class DefaultRequestFilterFactory<TRequestFilter, TRequestInfo> : IRequestFilterFactory<TRequestInfo>
        where TRequestInfo : IRequestInfo
        where TRequestFilter : IRequestFilter<TRequestInfo>, new()
    {
        /// <summary>
        /// Creates the request filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns>the new created request filer assosiated with this socketSession</returns>
        public IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return new TRequestFilter();
        }
    }
}
