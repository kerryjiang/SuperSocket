using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Command
{
    public class ReflectCommandLoader : ICommandLoader
    {
        #region ICommandLoader Members

        public IEnumerable<ICommand<TAppSession, TCommandInfo>> LoadCommands<TAppSession, TCommandInfo>(IAppServer appServer)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : ICommandInfo
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
                    throw new Exception("Failed to load defined command assemblies!", e);
                }
            }

            var commands = new List<ICommand<TAppSession, TCommandInfo>>();

            foreach (var assembly in commandAssemblies)
            {
                commands.AddRange(assembly.GetImplementedObjectsByInterface<ICommand<TAppSession, TCommandInfo>>());
            }

            return commands;
        }

        #endregion
    }
}
