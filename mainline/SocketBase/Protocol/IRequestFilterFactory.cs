using System;

namespace SuperSocket.SocketBase.Protocol
{
    public interface IRequestFilterFactory<TRequestInfo>
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
