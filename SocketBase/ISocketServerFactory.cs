using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for socket server factory
    /// </summary>
    public interface ISocketServerFactory
    {
        /// <summary>
        /// Creates the socket server instance.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the request info.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <param name="listeners">The listeners.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        ISocketServer CreateSocketServer<TPackageInfo>(IAppServer appServer, ListenerInfo[] listeners, IServerConfig config)
            where TPackageInfo : IPackageInfo;
    }
}
