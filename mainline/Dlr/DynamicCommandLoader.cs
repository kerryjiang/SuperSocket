using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Hosting;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Dlr
{
    /// <summary>
    /// Which is used for loading dynamic script file
    /// </summary>
    public class DynamicCommandLoader : CommandLoaderBase
    {
        private static ScriptRuntime m_ScriptRuntime;

        private static HashSet<string> m_CommandExtensions;

        private static Timer m_CommandDirCheckingTimer;

        private static readonly int m_CommandDirCheckingInterval = 1000 * 60 * 5;// 5 minutes

        private static List<DynamicCommandLoader> m_Loaders = new List<DynamicCommandLoader>();

        static DynamicCommandLoader()
        {
            m_ScriptRuntime = ScriptRuntime.CreateFromConfiguration();

            List<string> fileExtensions = new List<string>();

            foreach (var fxts in m_ScriptRuntime.Setup.LanguageSetups.Select(s => s.FileExtensions))
                fileExtensions.AddRange(fxts);

            m_CommandExtensions = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);

            m_ServerCommandStateLib = new Dictionary<string, ServerCommandState>(StringComparer.OrdinalIgnoreCase);
            m_CommandDirCheckingTimer = new Timer(OnCommandDirCheckingTimerCallback, null, m_CommandDirCheckingInterval, m_CommandDirCheckingInterval);
        }

        static IEnumerable<string> GetCommandFiles(string path, SearchOption option)
        {
            return Directory.GetFiles(path, "*.*", option).Where(f => m_CommandExtensions.Contains(Path.GetExtension(f)));
        }

        private static void OnCommandDirCheckingTimerCallback(object state)
        {
            m_CommandDirCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                var commandDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command");
                var commonCommands = GetCommandFiles(commandDir, SearchOption.TopDirectoryOnly);

                foreach (var name in m_ServerCommandStateLib.Keys)
                {
                    var serverState = m_ServerCommandStateLib[name];

                    var commandSourceDict = serverState.Commands.ToDictionary(c => c.FilePath,
                        c => new CommandFileEntity { Command = c },
                        StringComparer.OrdinalIgnoreCase);

                    var serverCommands = commonCommands.ToList();
                    
                    var serverCommandDir = Path.Combine(commandDir, name);

                    if (Directory.Exists(serverCommandDir))
                    {
                        serverCommands.AddRange(GetCommandFiles(serverCommandDir, SearchOption.TopDirectoryOnly));
                    }

                    List<CommandUpdateInfo<CommandFileInfo>> updatedCommands = new List<CommandUpdateInfo<CommandFileInfo>>();

                    foreach (var c in serverCommands)
                    {
                        var lastUpdatedTime = File.GetLastWriteTime(c);

                        CommandFileEntity commandEntity;

                        if (commandSourceDict.TryGetValue(c, out commandEntity))
                        {
                            commandEntity.Processed = true;

                            if (commandEntity.Command.LastUpdatedTime != lastUpdatedTime)
                            {
                                //update command's last updated time in dictionary
                                commandEntity.Command.LastUpdatedTime = lastUpdatedTime;

                                updatedCommands.Add(new CommandUpdateInfo<CommandFileInfo>
                                    {
                                        UpdateAction = CommandUpdateAction.Update,
                                        Command = new CommandFileInfo
                                            {
                                                FilePath = c,
                                                LastUpdatedTime = lastUpdatedTime
                                            }
                                    });
                            }
                        }
                        else
                        {
                            commandSourceDict.Add(c, new CommandFileEntity
                                {
                                    Command = new CommandFileInfo
                                        {
                                            FilePath = c,
                                            LastUpdatedTime = lastUpdatedTime
                                        },
                                    Processed = true
                                });

                            updatedCommands.Add(new CommandUpdateInfo<CommandFileInfo>
                                {
                                    UpdateAction = CommandUpdateAction.Add,
                                    Command = new CommandFileInfo
                                    {
                                        FilePath = c,
                                        LastUpdatedTime = lastUpdatedTime
                                    }
                                });
                        }
                    }

                    foreach (var cmd in commandSourceDict.Values.Where(e => !e.Processed))
                    {
                        updatedCommands.Add(new CommandUpdateInfo<CommandFileInfo>
                            {
                                UpdateAction = CommandUpdateAction.Remove,
                                Command = new CommandFileInfo
                                {
                                    FilePath = cmd.Command.FilePath,
                                    LastUpdatedTime = cmd.Command.LastUpdatedTime
                                }
                            });
                    }

                    if (updatedCommands.Count > 0)
                    {
                        serverState.Commands = commandSourceDict.Values.Where(e => e.Processed).Select(e => e.Command).ToList();
                        serverState.CommandUpdater(updatedCommands);
                    }
                }
            }
            catch (Exception e)
            {
                OnGlobalError(e);
            }
            finally
            {
                m_CommandDirCheckingTimer.Change(m_CommandDirCheckingInterval, m_CommandDirCheckingInterval);
            }
        }

        class CommandFileEntity
        {
            public CommandFileInfo Command { get; set; }
            public bool Processed { get; set; }
        }

        class CommandFileInfo
        {
            public string FilePath { get; set; }
            public DateTime LastUpdatedTime { get; set; }
        }

        class ServerCommandState
        {
            public List<CommandFileInfo> Commands { get; set; }
            public Action<IEnumerable<CommandUpdateInfo<CommandFileInfo>>> CommandUpdater { get; set; }
        }

        private static Dictionary<string, ServerCommandState> m_ServerCommandStateLib;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandLoader"/> class.
        /// </summary>
        public DynamicCommandLoader()
        {
            m_Loaders.Add(this);
        }

        private IEnumerable<CommandUpdateInfo<ICommand>> GetUpdatedCommands(IEnumerable<CommandUpdateInfo<CommandFileInfo>> updatedCommandFiles)
        {
            var updatedCommands = new List<CommandUpdateInfo<ICommand>>();

            foreach (var commandFile in updatedCommandFiles)
            {
                if (commandFile.UpdateAction == CommandUpdateAction.Remove)
                {
                    updatedCommands.Add(new CommandUpdateInfo<ICommand>
                    {
                        Command = (ICommand)Activator.CreateInstance(m_MockupCommandType, Path.GetFileNameWithoutExtension(commandFile.Command.FilePath)),
                        UpdateAction = commandFile.UpdateAction
                    });
                }

                try
                {
                    var command = (ICommand)Activator.CreateInstance(m_DynamicCommandType, m_ScriptRuntime, commandFile.Command.FilePath, commandFile.Command.LastUpdatedTime);

                    updatedCommands.Add(new CommandUpdateInfo<ICommand>
                    {
                        Command = command,
                        UpdateAction = commandFile.UpdateAction
                    });
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load command file: " + commandFile.Command.FilePath + "!", e));
                    continue;
                }
            }

            return updatedCommands;
        }

        private static void OnGlobalError(Exception e)
        {
            foreach (var l in m_Loaders)
            {
                l.OnError(e);
            }
        }

        private IAppServer m_AppServer;

        private Type m_DynamicCommandType;

        private Type m_MockupCommandType;

        /// <summary>
        /// Initializes with the specified app server.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public override bool Initialize<TCommand>(IAppServer appServer)
        {
            m_AppServer = appServer;

            var genericParameterTypes = typeof(TCommand).GetGenericArguments();
            m_DynamicCommandType = typeof(DynamicCommand<,>).MakeGenericType(genericParameterTypes);
            m_MockupCommandType = typeof(MockupCommand<,>).MakeGenericType(genericParameterTypes);
            return true;
        }

        /// <summary>
        /// Tries to load commands.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns></returns>
        public override bool TryLoadCommands(out IEnumerable<ICommand> commands)
        {
            var outputCommands = new List<ICommand>();

            if (m_ServerCommandStateLib.ContainsKey(m_AppServer.Name))
            {
                OnError("This server's commands have been loaded already!");
                commands = outputCommands;
                return false;
            }

            var serverCommandState = new ServerCommandState
            {
                CommandUpdater = (o) => OnUpdated(GetUpdatedCommands(o)),
                Commands = new List<CommandFileInfo>()
            };


            var commandDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command");
            var serverCommandDir = Path.Combine(commandDir, m_AppServer.Name);

            if (!Directory.Exists(commandDir))
            {
                commands = outputCommands;
                return true;
            }

            List<string> commandFiles = new List<string>();

            commandFiles.AddRange(GetCommandFiles(commandDir, SearchOption.TopDirectoryOnly));

            if (Directory.Exists(serverCommandDir))
            {
                commandFiles.AddRange(GetCommandFiles(serverCommandDir, SearchOption.TopDirectoryOnly));
            }

            if (!commandFiles.Any())
            {
                commands = outputCommands;
                return true;
            }

            foreach (var file in commandFiles)
            {
                ICommand command;

                try
                {
                    var lastUpdatedTime = File.GetLastWriteTime(file);
                    command = (ICommand)Activator.CreateInstance(m_DynamicCommandType, m_ScriptRuntime, file, lastUpdatedTime);
                    serverCommandState.Commands.Add(new CommandFileInfo
                    {
                        FilePath = file,
                        LastUpdatedTime = lastUpdatedTime
                    });

                    outputCommands.Add(command);
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load command file: " + file + "!", e));
                }
            }

            m_ServerCommandStateLib.Add(m_AppServer.Name, serverCommandState);

            commands = outputCommands;

            return true;
        }
    }
}