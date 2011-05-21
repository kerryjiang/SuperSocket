using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Command
{
    public class ReflectCommandLoader<TCommand> : ICommandLoader<TCommand>
        where TCommand : class
    {
        private Assembly[] m_CommandAssemblies;

        public ReflectCommandLoader(IEnumerable<Assembly> assemblies)
        {
            m_CommandAssemblies = assemblies.Where(a => a != null).ToArray();
        }

        #region ICommandLoader Members

        public IEnumerable<TCommand> LoadCommands()
        {
            var commands = new List<TCommand>();

            foreach (var assembly in m_CommandAssemblies)
            {
                commands.AddRange(assembly.GetImplementedObjectsByInterface<TCommand>());
            }

            return commands;
        }

        #endregion
    }
}
