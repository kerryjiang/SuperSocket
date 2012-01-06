using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// The interface you must implement for your custom protocol
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface ICustomProtocol<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Creates the command reader according the appServer.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        ICommandReader<TRequestInfo> CreateCommandReader(IAppServer appServer);
    }
}
