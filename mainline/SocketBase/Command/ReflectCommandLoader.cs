using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SuperSocket.SocketBase.Command
{
    internal class ReflectCommandLoader<TAppSession, TCommandInfo> : ICommandLoader<TAppSession, TCommandInfo>
        where TCommandInfo : ICommandInfo
        where TAppSession : IAppSession, IAppSession<TCommandInfo>, new()
    {
        private Assembly m_CommandAssembly;

        public ReflectCommandLoader()
            : this(typeof(TAppSession).Assembly)
        {

        }

        public ReflectCommandLoader(Assembly assembly)
        {
            m_CommandAssembly = assembly;
        }


        #region ICommandLoader Members

        public List<ICommand<TAppSession, TCommandInfo>> LoadCommands()            
        {
            Type commandType = typeof(ICommand<TAppSession, TCommandInfo>);
            Type[] arrType = m_CommandAssembly.GetExportedTypes();

            List<ICommand<TAppSession, TCommandInfo>> commands = new List<ICommand<TAppSession, TCommandInfo>>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentCommandType = arrType[i];

                if (currentCommandType.IsAbstract)
                    continue;

                var commandInterface = currentCommandType.GetInterfaces().SingleOrDefault(x => x == commandType);

                if (commandInterface != null)
                {
                    commands.Add(currentCommandType.GetConstructor(new Type[0]).Invoke(new object[0]) as ICommand<TAppSession, TCommandInfo>);
                }
            }

            return commands;
        }

        #endregion
    }
}
