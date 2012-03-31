using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using System.Net;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Default socket server factory
    /// </summary>
    public class SocketServerFactory : ISocketServerFactory
    {
        private static ISocketServerFactory m_Instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ISocketServerFactory Instance
        {
            get { return m_Instance; }
        }

        static SocketServerFactory()
        {
            m_Instance = new SocketServerFactory();
        }

        private const string m_SecurityNone = "None";

        #region ISocketServerFactory Members

        /// <summary>
        /// Creates the socket server.
        /// </summary>
        /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <param name="listeners">The listeners.</param>
        /// <param name="config">The config.</param>
        /// <param name="requestFilterFactory">The request filter factory.</param>
        /// <returns></returns>
        public ISocketServer CreateSocketServer<TRequestInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config, IRequestFilterFactory<TRequestInfo> requestFilterFactory)
            where TRequestInfo : IRequestInfo
        {
            if (requestFilterFactory == null)
                throw new ArgumentNullException("requestFilterFactory");

            switch(config.Mode)
            {
                case(SocketMode.Tcp):
                    return new AsyncSocketServer(appServer, listeners);
                default:
                    throw new NotSupportedException("Unsupported SocketMode:" + config.Mode);
            }
        }

        #endregion
    }
}
