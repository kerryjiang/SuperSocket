using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// DefaultreceiveFilterFactory
    /// </summary>
    /// <typeparam name="TReceiveFilter">The type of the Receive filter.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public class DefaultReceiveFilterFactory<TReceiveFilter, TRequestInfo> : IReceiveFilterFactory<TRequestInfo>
        where TRequestInfo : IRequestInfo
        where TReceiveFilter : IReceiveFilter<TRequestInfo>, new()
    {
        /// <summary>
        /// Creates the Receive filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns>
        /// the new created request filer assosiated with this socketSession
        /// </returns>
        public virtual IReceiveFilter<TRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new TReceiveFilter();
        }
    }
}
