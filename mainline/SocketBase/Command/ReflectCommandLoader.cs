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
                    throw new Exception("Failed to load defined command assemblies!", e);
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
    }
}
