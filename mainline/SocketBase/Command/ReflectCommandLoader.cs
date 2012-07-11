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
    public class ReflectCommandLoader : CommandLoaderBase
    {
        //#region ICommandLoader Members

        ///// <summary>
        ///// Loads the commands for specific server.
        ///// </summary>
        ///// <typeparam name="TAppSession">The type of the app session.</typeparam>
        ///// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
        ///// <param name="appServer">The app server.</param>
        ///// <param name="commandRegister">The command register.</param>
        ///// <param name="commandUpdater">The command updater.</param>
        ///// <returns></returns>
        //public bool LoadCommands<TAppSession, TRequestInfo>(IAppServer appServer, Func<ICommand<TAppSession, TRequestInfo>, bool> commandRegister, Action<IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>>> commandUpdater)
        //    where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        //    where TRequestInfo : IRequestInfo
        //{
        //    var commandAssemblies = new List<Assembly> { appServer.GetType().Assembly };

        //    string commandAssembly = appServer.Config.Options.GetValue("commandAssembly");

        //    if (!string.IsNullOrEmpty(commandAssembly))
        //    {
        //        try
        //        {
        //            var definedAssemblies = AssemblyUtil.GetAssembliesFromString(commandAssembly);

        //            if (definedAssemblies.Any())
        //                commandAssemblies.AddRange(definedAssemblies);
        //        }
        //        catch (Exception e)
        //        {
        //            OnError(new Exception("Failed to load defined command assemblies!", e));
        //            return false;
        //        }
        //    }

        //    foreach (var assembly in commandAssemblies)
        //    {
        //        foreach (var c in assembly.GetImplementedObjectsByInterface<ICommand<TAppSession, TRequestInfo>>())
        //        {
        //            if (!commandRegister(c))
        //                return false;
        //        }
        //    }

        //    return true;
        //}

        //#endregion

        private Type m_CommandType;

        private IAppServer m_AppServer;

        /// <summary>
        /// Initializes the command loader
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public override bool Initialize<TCommand>(IAppServer appServer)
        {
            m_CommandType = typeof(TCommand);
            m_AppServer = appServer;
            return true;
        }

        /// <summary>
        /// Tries to load commands.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        public override bool TryLoadCommands(out IEnumerable<ICommand> commands)
        {
            commands = null;

            var commandAssemblies = new List<Assembly>();

            if (m_AppServer.GetType().Assembly != this.GetType().Assembly)
                commandAssemblies.Add(m_AppServer.GetType().Assembly);

            string commandAssembly = m_AppServer.Config.Options.GetValue("commandAssembly");

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

            if (!commandAssemblies.Any())
            {
                OnError("You should configure the commandAssembly value!");
                return false;
            }

            var outputCommands = new List<ICommand>();

            foreach (var assembly in commandAssemblies)
            {
                try
                {
                    outputCommands.AddRange(assembly.GetImplementedObjectsByInterface<ICommand>(m_CommandType));
                }
                catch (Exception exc)
                {
                    OnError(new Exception(string.Format("Failed to get commands from the assembly {0}!", assembly.FullName), exc));
                    return false;
                }
            }

            commands = outputCommands;

            return true;
        }
    }
}
