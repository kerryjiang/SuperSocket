using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SuperSocket.SocketBase.Command
{
    public class ReflectCommandLoader<TCommandInfo> : ICommandLoader<TCommandInfo>
        where TCommandInfo : class
    {
        private Assembly m_CommandAssembly;

        public ReflectCommandLoader(Assembly assembly)
        {
            m_CommandAssembly = assembly;
        }

        #region ICommandLoader Members

        public List<TCommandInfo> LoadCommands()            
        {
            Type commandType = typeof(TCommandInfo);
            Type[] arrType = m_CommandAssembly.GetExportedTypes();

            var commands = new List<TCommandInfo>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentCommandType = arrType[i];

                if (currentCommandType.IsAbstract)
                    continue;

                var commandInterface = currentCommandType.GetInterfaces().SingleOrDefault(x => x == commandType);

                if (commandInterface != null)
                {
                    commands.Add(currentCommandType.GetConstructor(new Type[0]).Invoke(new object[0]) as TCommandInfo);
                }
            }

            return commands;
        }

        #endregion
    }
}
