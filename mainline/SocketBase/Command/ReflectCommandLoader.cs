using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// A command loader which loads commands from assembly by reflection
    /// </summary>
    public class ReflectCommandLoader : ICommandLoader
    {
        #region ICommandLoader Members

        /// <summary>
        /// Loads the commands for specific server.
        /// </summary>
        /// <typeparam name="TAppSession">The type of the app session.</typeparam>
        /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <param name="commandRegister">The command register.</param>
        /// <param name="commandUpdater">The command updater.</param>
        /// <returns></returns>
        public bool LoadCommands<TAppSession, TRequestInfo>(IAppServer appServer, Func<ICommand<TAppSession, TRequestInfo>, bool> commandRegister, Action<IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>>> commandUpdater)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : IRequestInfo
        {
            var commandAssemblies = new List<Assembly> { appServer.GetType().Assembly };

            string commandAssembly = appServer.Config.Options.GetValue("commandAssembly");

            if (!string.IsNullOrEmpty(commandAssembly))
            {
                try
                {
                    var definedAssemblies = AssemblyUtil.GetAssembliesFromString(commandAssembly);

                    if (definedAssemblies.Any())
                        commandAssemblies.AddRange(definedAssemblies);
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load defined command assemblies!", e));
                    return false;
                }
            }

            foreach (var assembly in commandAssemblies)
            {
                foreach (var c in assembly.GetImplementedObjectsByInterface<ICommand<TAppSession, TRequestInfo>>())
                {
                    if (!commandRegister(c))
                        return false;
                }
            }

            return true;
        }

        #endregion

        private void OnError(Exception e)
        {
            var handler = Error;

            if (handler != null)
                handler(this, new ErrorEventArgs(e));
        }


        /// <summary>
        /// Occurs when [error].
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;
    }
}
