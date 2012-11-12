using System;
using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.Protocol
{
    /// <summary>
    /// ReceiveFilterFactory for CountSpliterReceiveFilter
    /// </summary>
    /// <typeparam name="TRequestFilter">The type of the Receive filter.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public class CountSpliterReceiveFilterFactory<TRequestFilter, TRequestInfo> : IReceiveFilterFactory<TRequestInfo>
        where TRequestFilter : CountSpliterReceiveFilter<TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IReceiveFilter<TRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            var config = appServer.Config;

            if(config.MaxRequestLength > config.ReceiveBufferSize)
                throw new Exception("ReceiveBufferSize cannot smaller than MaxRequestLength in this protocol.");

            return new TRequestFilter();
        }
    }

    /// <summary>
    /// ReceiveFilterFactory for CountSpliterReceiveFilter
    /// </summary>
    /// <typeparam name="TRequestFilter">The type of the Receive filter.</typeparam>
    public class CountSpliterReceiveFilterFactory<TRequestFilter> : CountSpliterReceiveFilterFactory<TRequestFilter, StringRequestInfo>
        where TRequestFilter : CountSpliterReceiveFilter<StringRequestInfo>, new()
    {

    }

    /// <summary>
    /// receiveFilterFactory for CountSpliterRequestFilter
    /// </summary>
    public class  CountSpliterReceiveFilterFactory : IReceiveFilterFactory<StringRequestInfo>
    {
        private readonly byte m_Spliter;

        private readonly int m_SpliterCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilterFactory"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="count">The count.</param>
        public CountSpliterReceiveFilterFactory(byte spliter, int count)
        {
            m_Spliter = spliter;
            m_SpliterCount = count;
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IReceiveFilter<StringRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new CountSpliterReceiveFilter(m_Spliter, m_SpliterCount);
        }
    }
}
