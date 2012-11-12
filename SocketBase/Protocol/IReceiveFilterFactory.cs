using System;
using System.Net;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Receive filter factory interface
    /// </summary>
    public interface IReceiveFilterFactory
    {

    }
    /// <summary>
    /// Receive filter factory interface
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IReceiveFilterFactory<TRequestInfo> : IReceiveFilterFactory
        where TRequestInfo : IRequestInfo
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
        IReceiveFilter<TRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint);
    }
}
