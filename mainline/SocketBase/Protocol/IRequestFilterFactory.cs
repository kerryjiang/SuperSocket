using System;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Request filter factory interface
    /// </summary>
    public interface IRequestFilterFactory
    {

    }
    /// <summary>
    /// Request filter factory interface
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IRequestFilterFactory<TRequestInfo> : IRequestFilterFactory
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Creates the request filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="socketSession">The socket session.</param>
        /// <returns>the new created request filer assosiated with this socketSession</returns>
        IRequestFilter<TRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession);
    }
}
