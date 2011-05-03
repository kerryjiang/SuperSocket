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
    /// <typeparam name="TCommandInfo">The type of the command info.</typeparam>
    public interface ICustomProtocol<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        /// <summary>
        /// Creates the command reader according the appServer.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        ICommandReader<TCommandInfo> CreateCommandReader(IAppServer appServer);
    }
}
