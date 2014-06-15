using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// DefaultreceiveFilterFactory
    /// </summary>
    /// <typeparam name="TReceiveFilter">The type of the Receive filter.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the request info.</typeparam>
    public class DefaultReceiveFilterFactory<TReceiveFilter, TPackageInfo> : IReceiveFilterFactory<TPackageInfo>
        where TPackageInfo : IPackageInfo
        where TReceiveFilter : IReceiveFilter<TPackageInfo>, new()
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
        public virtual IReceiveFilter<TPackageInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new TReceiveFilter();
        }
    }
}
