using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Provider
{
    /// <summary>
    /// ProviderKey
    /// </summary>
    [Serializable]
    public class ProviderKey
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; private set; }

        private ProviderKey()
        {

        }

        static ProviderKey()
        {
            Service = new ProviderKey { Name = "Service", Type = typeof(IAppServer) };
            SocketServerFactory = new ProviderKey { Name = "SocketServerFactory", Type = typeof(ISocketServerFactory) };
            ConnectionFilter = new ProviderKey { Name = "ConnectionFilter", Type = typeof(IConnectionFilter) };
            LogFactory = new ProviderKey { Name = "LogFactory", Type = typeof(ILogFactory) };
            RequestFilterFactory = new ProviderKey { Name = "RequestFilterFactory", Type = typeof(IRequestFilterFactory) };
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        public static ProviderKey Service { get; private set; }

        /// <summary>
        /// Gets the socket server factory.
        /// </summary>
        public static ProviderKey SocketServerFactory { get; private set; }

        /// <summary>
        /// Gets the connection filter.
        /// </summary>
        public static ProviderKey ConnectionFilter { get; private set; }

        /// <summary>
        /// Gets the log factory.
        /// </summary>
        public static ProviderKey LogFactory { get; private set; }

        /// <summary>
        /// Gets the request filter factory.
        /// </summary>
        public static ProviderKey RequestFilterFactory { get; private set; }
    }
}
