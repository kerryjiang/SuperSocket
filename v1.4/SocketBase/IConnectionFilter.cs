using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Net;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The basic interface of connection filter
    /// </summary>
    public interface IConnectionFilter
    {
        /// <summary>
        /// Initializes the connection filter
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        bool Initialize(string name, NameValueCollection options);

        /// <summary>
        /// Gets the name of the filter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Whether allows the connect according the remote endpoint
        /// </summary>
        /// <param name="remoteAddress">The remote address.</param>
        /// <returns></returns>
        bool AllowConnect(IPEndPoint remoteAddress);
    }
}

