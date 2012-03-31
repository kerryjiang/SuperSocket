using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// Command loader's interface
    /// </summary>
    public interface ICommandLoader
    {
        /// <summary>
        /// Loads the commands for specific server.
        /// </summary>
        /// <typeparam name="TAppSession">The type of the app session.</typeparam>
        /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <param name="commandRegister">The command register.</param>
        /// <param name="commandUpdater">The command updater.</param>
        /// <returns></returns>
        bool LoadCommands<TAppSession, TRequestInfo>(IAppServer appServer, Func<ICommand<TAppSession, TRequestInfo>, bool> commandRegister, Action<IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>>> commandUpdater)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : IRequestInfo;
    }
}
