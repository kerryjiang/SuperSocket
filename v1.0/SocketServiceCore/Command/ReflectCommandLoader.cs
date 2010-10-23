using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SuperSocket.SocketServiceCore.Command
{
    internal class ReflectCommandLoader : ICommandLoader
    {
        #region ICommandLoader Members

        public List<ICommand<TAppSession>> LoadCommands<TAppSession>() where TAppSession : IAppSession, IAppSession<TAppSession>, new()
        {
            Type commandType = typeof(ICommand<TAppSession>);
            Assembly asm = typeof(TAppSession).Assembly;
            Type[] arrType = asm.GetExportedTypes();

            List<ICommand<TAppSession>> commands = new List<ICommand<TAppSession>>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentCommandType = arrType[i];
                var commandInterface = currentCommandType.GetInterfaces().SingleOrDefault(x => x == commandType);

                if (commandInterface != null)
                {
                    commands.Add(currentCommandType.GetConstructor(new Type[0]).Invoke(new object[0]) as ICommand<TAppSession>);
                }
            }

            return commands;
        }

        #endregion
    }
}
