using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// ReceiveFilterFactory for CountSpliterReceiveFilter
    /// </summary>
    /// <typeparam name="TReceiveFilter">The type of the Receive filter.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the request info.</typeparam>
    public class CountSpliterReceiveFilterFactory<TReceiveFilter, TPackageInfo> : IReceiveFilterFactory<TPackageInfo>
        where TReceiveFilter : CountSpliterReceiveFilter<TPackageInfo>, new()
        where TPackageInfo : IPackageInfo
    {
        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IReceiveFilter<TPackageInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            var config = appServer.Config;

            if (config.MaxRequestLength > config.ReceiveBufferSize)
                throw new Exception("ReceiveBufferSize cannot smaller than MaxRequestLength in this protocol.");

            return new TReceiveFilter();
        }
    }

    /// <summary>
    /// ReceiveFilterFactory for CountSpliterReceiveFilter
    /// </summary>
    /// <typeparam name="TReceiveFilter">The type of the Receive filter.</typeparam>
    public class CountSpliterReceiveFilterFactory<TReceiveFilter> : CountSpliterReceiveFilterFactory<TReceiveFilter, StringPackageInfo>
        where TReceiveFilter : CountSpliterReceiveFilter<StringPackageInfo>, new()
    {

    }

    /// <summary>
    /// receiveFilterFactory for CountSpliterRequestFilter
    /// </summary>
    public class CountSpliterReceiveFilterFactory : IReceiveFilterFactory<StringPackageInfo>
    {
        private readonly byte[] m_Spliter;

        private readonly int m_SpliterCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilterFactory"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="count">The count.</param>
        public CountSpliterReceiveFilterFactory(byte spliter, int count)
        {
            m_Spliter = new byte[] { spliter };
            m_SpliterCount = count;
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appSession">The app session.</param>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <returns></returns>
        public IReceiveFilter<StringPackageInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            return new CountSpliterReceiveFilter(m_Spliter, m_SpliterCount);
        }
    }
}
