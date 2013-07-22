using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Dlr
{
    /// <summary>
    /// DynamicCommandLoader
    /// </summary>
    public class DynamicCommandLoader<TCommand> : DynamicCommandLoaderBase<TCommand>
        where TCommand : ICommand
    {
        private HashSet<string> m_ScriptFileExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandLoader{TCommand}"/> class.
        /// </summary>
        public DynamicCommandLoader()
            : base()
        {
            LoadScriptFileExtensions();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandLoader{TCommand}"/> class.
        /// </summary>
        /// <param name="scriptRuntime">The script runtime.</param>
        public DynamicCommandLoader(ScriptRuntime scriptRuntime)
            : base(scriptRuntime)
        {
            LoadScriptFileExtensions();
        }

        private void LoadScriptFileExtensions()
        {
            List<string> fileExtensions = new List<string>();

            foreach (var fxts in ScriptRuntime.Setup.LanguageSetups.Select(s => s.FileExtensions))
                fileExtensions.AddRange(fxts);

            m_ScriptFileExtensions = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetCommandFiles(string path, SearchOption option)
        {
            return Directory.GetFiles(path, "*.*", option).Where(f => m_ScriptFileExtensions.Contains(Path.GetExtension(f)));
        }

        /// <summary>
        /// Gets the script sources.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        protected override IEnumerable<IScriptSource> GetScriptSources(IRootConfig rootConfig, IAppServer appServer)
        {
            var sources = new List<IScriptSource>();

            string commandDir = string.Empty;
            string serverCommandDir = string.Empty;

            var commandDirSearchOption = SearchOption.TopDirectoryOnly;

            if (rootConfig.Isolation == IsolationMode.None)
            {
                commandDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command");
                serverCommandDir = Path.Combine(commandDir, appServer.Name);
            }
            else
            {
                commandDirSearchOption = SearchOption.AllDirectories;
                commandDir = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "Command");
                serverCommandDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command");
            }

            List<string> commandFiles = new List<string>();

            if (Directory.Exists(commandDir))
                commandFiles.AddRange(GetCommandFiles(commandDir, commandDirSearchOption));

            if (Directory.Exists(serverCommandDir))
                commandFiles.AddRange(GetCommandFiles(serverCommandDir, SearchOption.AllDirectories));

            if (!commandFiles.Any())
            {
                return sources;
            }

            foreach (var file in commandFiles)
            {
                sources.Add(new FileScriptSource(file));
            }

            return sources;
        }
    }
}
