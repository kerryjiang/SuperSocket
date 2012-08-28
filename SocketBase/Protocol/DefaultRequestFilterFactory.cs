using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns>
        /// the new created request filer assosiated with this socketSession
        /// </returns>
        public virtual IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new TRequestFilter();
        }
    }
}
