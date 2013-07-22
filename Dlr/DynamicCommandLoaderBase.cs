using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Hosting;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Dlr
{
    /// <summary>
    /// Which is used for loading dynamic script file
    /// </summary>
    public abstract class DynamicCommandLoaderBase<TCommand> : CommandLoaderBase<TCommand>
        where TCommand : ICommand
    {
        class CommandFileEntity
        {
            public IScriptSource Source { get; set; }
            public bool Processed { get; set; }
        }

        class ServerCommandState
        {
            public List<IScriptSource> Sources { get; set; }
            public Action<IEnumerable<CommandUpdateInfo<IScriptSource>>> CommandUpdater { get; set; }
        }

        /// <summary>
        /// Gets the script runtime.
        /// </summary>
        protected ScriptRuntime ScriptRuntime { get; private set; }

        private Timer m_ScriptSourceCheckingTimer;

        private static readonly int m_ScriptSourceCheckingInterval = 1000 * 60 * 5;// 5 minutes

        private ServerCommandState m_ServerCommandState;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandLoaderBase{TCommand}"/> class.
        /// </summary>
        public DynamicCommandLoaderBase()
            : this(ScriptRuntime.CreateFromConfiguration())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandLoaderBase{TCommand}"/> class.
        /// </summary>
        /// <param name="scriptRuntime">The script runtime.</param>
        public DynamicCommandLoaderBase(ScriptRuntime scriptRuntime)
        {
            ScriptRuntime = scriptRuntime;
            m_ScriptSourceCheckingTimer = new Timer(OnScriptSourceCheckingTimerCallback, null, m_ScriptSourceCheckingInterval, m_ScriptSourceCheckingInterval);
        }

        /// <summary>
        /// Gets the script sources.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        protected abstract IEnumerable<IScriptSource> GetScriptSources(IRootConfig rootConfig, IAppServer appServer);

        private IEnumerable<CommandUpdateInfo<TCommand>> GetUpdatedCommands(IEnumerable<CommandUpdateInfo<IScriptSource>> updatedSources)
        {
            var updatedCommands = new List<CommandUpdateInfo<TCommand>>();

            foreach (var source in updatedSources)
            {
                if (source.UpdateAction == CommandUpdateAction.Remove)
                {
                    updatedCommands.Add(new CommandUpdateInfo<TCommand>
                    {
                        Command = (TCommand)Activator.CreateInstance(m_MockupCommandType, source),
                        UpdateAction = source.UpdateAction
                    });
                }

                try
                {
                    var command = (TCommand)Activator.CreateInstance(m_DynamicCommandType, ScriptRuntime, source);

                    updatedCommands.Add(new CommandUpdateInfo<TCommand>
                    {
                        Command = command,
                        UpdateAction = source.UpdateAction
                    });
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load command source: " + source.Command.Tag + "!", e));
                    continue;
                }
            }

            return updatedCommands;
        }


        private IAppServer m_AppServer;

        private IRootConfig m_RootConfig;

        private Type m_DynamicCommandType;

        private Type m_MockupCommandType;

        /// <summary>
        /// Initializes the command loader by the root config and the appserver instance.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public override bool Initialize(IRootConfig rootConfig, IAppServer appServer)
        {
            m_RootConfig = rootConfig;
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
        public override bool TryLoadCommands(out IEnumerable<TCommand> commands)
        {
            var outputCommands = new List<TCommand>();

            if (m_ServerCommandState != null)
            {
                OnError("This server's commands have been loaded already!");
                commands = outputCommands;
                return false;
            }

            var serverCommandState = new ServerCommandState
            {
                CommandUpdater = (o) => OnUpdated(GetUpdatedCommands(o)),
                Sources = new List<IScriptSource>()
            };

            m_ServerCommandState = serverCommandState;

            var scriptSources = GetScriptSources(m_RootConfig, m_AppServer);

            foreach (var source in scriptSources)
            {
                TCommand command;

                try
                {
                    command = (TCommand)Activator.CreateInstance(m_DynamicCommandType, ScriptRuntime, source);
                    serverCommandState.Sources.Add(source);
                    outputCommands.Add(command);
                }
                catch (Exception e)
                {
                    OnError(new Exception("Failed to load command source: " + source.Tag + "!", e));
                }
            }

            commands = outputCommands;

            return true;
        }

        private void OnScriptSourceCheckingTimerCallback(object state)
        {
            m_ScriptSourceCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                var serverState = m_ServerCommandState;

                var commandSourceDict = serverState.Sources.ToDictionary(c => c.Tag,
                    c => new CommandFileEntity { Source = c },
                    StringComparer.OrdinalIgnoreCase);

                var commandSources = GetScriptSources(m_RootConfig, m_AppServer);

                List<CommandUpdateInfo<IScriptSource>> updatedSources = new List<CommandUpdateInfo<IScriptSource>>();

                foreach (var c in commandSources)
                {
                    var lastUpdatedTime = c.LastUpdatedTime;

                    CommandFileEntity commandEntity;

                    if (commandSourceDict.TryGetValue(c.Tag, out commandEntity))
                    {
                        commandEntity.Processed = true;

                        if (commandEntity.Source.LastUpdatedTime != lastUpdatedTime)
                        {
                            //update command's last updated time in dictionary
                            commandEntity.Source = c;

                            updatedSources.Add(new CommandUpdateInfo<IScriptSource>
                            {
                                UpdateAction = CommandUpdateAction.Update,
                                Command = c
                            });
                        }
                    }
                    else
                    {
                        commandSourceDict.Add(c.Tag, new CommandFileEntity
                        {
                            Source = c,
                            Processed = true
                        });

                        updatedSources.Add(new CommandUpdateInfo<IScriptSource>
                        {
                            UpdateAction = CommandUpdateAction.Add,
                            Command = c
                        });
                    }
                }

                foreach (var cmd in commandSourceDict.Values.Where(e => !e.Processed))
                {
                    updatedSources.Add(new CommandUpdateInfo<IScriptSource>
                    {
                        UpdateAction = CommandUpdateAction.Remove,
                        Command = cmd.Source
                    });
                }

                if (updatedSources.Count > 0)
                {
                    serverState.Sources = commandSourceDict.Values.Where(e => e.Processed).Select(e => e.Source).ToList();
                    serverState.CommandUpdater(updatedSources);
                }
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                m_ScriptSourceCheckingTimer.Change(m_ScriptSourceCheckingInterval, m_ScriptSourceCheckingInterval);
            }
        }
    }
}