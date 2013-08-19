using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// A command loader which loads commands from assembly by reflection
    /// </summary>
    public class ReflectCommandLoader<TCommand> : CommandLoaderBase<TCommand>
        where TCommand : class, ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectCommandLoader{TCommand}"/> class.
        /// </summary>
        public ReflectCommandLoader()
        {

        }

        private IAppServer m_AppServer;

        /// <summary>
        /// Initializes the command loader by the root config and the server instance.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public override bool Initialize(IRootConfig rootConfig, IAppServer appServer)
        {
            m_AppServer = appServer;
            return true;
        }

        /// <summary>
        /// Tries to load commands.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        public override bool TryLoadCommands(out IEnumerable<TCommand> commands)
        {
            commands = null;

            var commandAssemblies = new List<Assembly>();

            if (m_AppServer.GetType().Assembly != this.GetType().Assembly)
                commandAssemblies.Add(m_AppServer.GetType().Assembly);

            string commandAssembly = m_AppServer.Config.Options.GetValue("commandAssembly");

            if (!string.IsNullOrEmpty(commandAssembly))
            {
                OnError("The configuration attribute 'commandAssembly' is not in used, please try to use the child node 'commandAssemblies' instead!");
                return false;
            }


            if (m_AppServer.Config.CommandAssemblies != null && m_AppServer.Config.CommandAssemblies.Any())
            {
                try
                {
                    var definedAssemblies = AssemblyUtil.GetAssembliesFromStrings(m_AppServer.Config.CommandAssemblies.Select(a => a.Assembly).ToArray());

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
                commandAssemblies.Add(Assembly.GetEntryAssembly());
            }

            var outputCommands = new List<TCommand>();

            foreach (var assembly in commandAssemblies)
            {
                try
                {
                    outputCommands.AddRange(assembly.GetImplementedObjectsByInterface<TCommand>());
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
