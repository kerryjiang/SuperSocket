using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Dlr
{
    public class DynamicCommandLoader : ICommandLoader
    {
        public IEnumerable<ICommand<TAppSession, TCommandInfo>> LoadCommands<TAppSession, TCommandInfo>(IAppServer appServer)
            where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
            where TCommandInfo : ICommandInfo
        {
            var commandDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command");
            var serverCommandDir = Path.Combine(commandDir, appServer.Name);

            if (!Directory.Exists(commandDir))
                return null;

            List<string> commandFiles = new List<string>();

            commandFiles.AddRange(Directory.GetFiles(commandDir, "*.py", SearchOption.TopDirectoryOnly));

            if (Directory.Exists(serverCommandDir))
            {
                commandFiles.AddRange(Directory.GetFiles(serverCommandDir, "*.py", SearchOption.TopDirectoryOnly));
            }

            if (!commandFiles.Any())
                return null;

            var commands = new List<ICommand<TAppSession, TCommandInfo>>(commandFiles.Count);

            foreach (var file in commandFiles)
            {
                DynamicCommand<TAppSession, TCommandInfo> command;

                try
                {
                    command = new DynamicCommand<TAppSession, TCommandInfo>(file);
                    commands.Add(command);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to load command file: " + file + "!", e);
                }
            }

            return commands;
        }
    }
}