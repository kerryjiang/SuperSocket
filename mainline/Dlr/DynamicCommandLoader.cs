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
    public class DynamicCommandLoader : ICommandLoader
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
        /// Loads the commands.
        /// </summary>
        /// <typeparam name="TAppSession">The type of the app session.</typeparam>
        /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
        /// <param name="appServer">The app server.</param>
        /// <param name="commandRegister">The command register.</param>
        /// <param name="commandUpdater">The command updater.</param>
        /// <returns></returns>
        public bool LoadCommands<TAppSession, TRequestInfo>(IAppServer appServer, Func<ICommand<TAppSession, TRequestInfo>, bool> commandRegister, Action<IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>>> commandUpdater)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : IRequestInfo
        {
            if (m_ServerCommandStateLib.ContainsKey(appServer.Name))
            {
                OnError(new Exception("This server's commands have been loaded already!"));
                return false;
            }

            ServerCommandState serverCommandState = new ServerCommandState
            {
                CommandUpdater = (o) =>
                {
                    commandUpdater(UpdateCommands<TAppSession, TRequestInfo>(appServer, o));
                },
                Commands = new List<CommandFileInfo>()
            };


            var commandDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command");
            var serverCommandDir = Path.Combine(commandDir, appServer.Name);

            if (!Directory.Exists(commandDir))
                return true;

            List<string> commandFiles = new List<string>();

            commandFiles.AddRange(GetCommandFiles(commandDir, SearchOption.TopDirectoryOnly));

            if (Directory.Exists(serverCommandDir))
            {
                commandFiles.AddRange(GetCommandFiles(serverCommandDir, SearchOption.TopDirectoryOnly));
            }

            if (!commandFiles.Any())
                return true;

            foreach (var file in commandFiles)
            {
                DynamicCommand<TAppSession, TRequestInfo> command;

                try
                {
                    var lastUpdatedTime = File.GetLastWriteTime(file);
                    command = new DynamicCommand<TAppSession, TRequestInfo>(m_ScriptRuntime, file, lastUpdatedTime);
                    serverCommandState.Commands.Add(new CommandFileInfo
                        {
                            FilePath = file,
                            LastUpdatedTime = lastUpdatedTime
                        });

                    if (!commandRegister(command))
                        return false;
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load command file: " + file + "!", e));
                }
            }

            m_ServerCommandStateLib.Add(appServer.Name, serverCommandState);

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandLoader"/> class.
        /// </summary>
        public DynamicCommandLoader()
        {
            m_Loaders.Add(this);
        }

        private IEnumerable<CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>> UpdateCommands<TAppSession, TRequestInfo>(IAppServer appServer, IEnumerable<CommandUpdateInfo<CommandFileInfo>> updatedCommands)
            where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
            where TRequestInfo : IRequestInfo
        {
            return updatedCommands.Select(c =>
            {
                if (c.UpdateAction == CommandUpdateAction.Remove)
                {
                    return new CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>
                    {
                        Command = new MockupCommand<TAppSession, TRequestInfo>(Path.GetFileNameWithoutExtension(c.Command.FilePath)),
                        UpdateAction = c.UpdateAction
                    };
                }

                try
                {
                    var command = new DynamicCommand<TAppSession, TRequestInfo>(m_ScriptRuntime, c.Command.FilePath, c.Command.LastUpdatedTime);

                    return new CommandUpdateInfo<ICommand<TAppSession, TRequestInfo>>
                    {
                        Command = command,
                        UpdateAction = c.UpdateAction
                    };
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load command file: " + c.Command.FilePath + "!", e));
                    return null;
                }
            });
        }

        private void OnError(Exception e)
        {
            var handler = Error;

            if (handler != null)
                handler(this, new SuperSocket.Common.ErrorEventArgs(e));
        }

        private static void OnGlobalError(Exception e)
        {
            foreach (var l in m_Loaders)
            {
                l.OnError(e);
            }
        }


        /// <summary>
        /// Occurs when [error].
        /// </summary>
        public event EventHandler<Common.ErrorEventArgs> Error;
    }
}