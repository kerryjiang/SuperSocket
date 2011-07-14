using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Dlr
{
    class DynamicCommand<TAppSession, TCommandInfo> : ICommand<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        private static ScriptRuntime m_ScriptRuntime;

        private Action<TAppSession, TCommandInfo> m_DynamicExecuteCommand;

        static DynamicCommand()
        {
            m_ScriptRuntime = ScriptRuntime.CreateFromConfiguration();
        }

        public DynamicCommand(string filePath, DateTime lastUpdatedTime)
        {
            FilePath = filePath;
            LastUpdatedTime = lastUpdatedTime;

            Name = Path.GetFileNameWithoutExtension(filePath);
            var scriptEngine = m_ScriptRuntime.GetEngineByFileExtension(Path.GetExtension(filePath));
            var scriptScope = scriptEngine.CreateScope();

            var scriptSource = scriptEngine.CreateScriptSourceFromFile(filePath);
            var compiledCode = scriptSource.Compile();
            compiledCode.Execute(scriptScope);

            Action<TAppSession, TCommandInfo> dynamicMethod;
            if (!scriptScope.TryGetVariable<Action<TAppSession, TCommandInfo>>("execute", out dynamicMethod))
                throw new Exception("Failed to find a command execution method in file: " + filePath);

            m_DynamicExecuteCommand = dynamicMethod;
        }

        #region ICommand<TAppSession,TCommandInfo> Members

        public virtual void ExecuteCommand(TAppSession session, TCommandInfo commandInfo)
        {
            m_DynamicExecuteCommand(session, commandInfo);
        }

        #endregion

        public string FilePath { get; private set; }

        public DateTime LastUpdatedTime { get; private set; }

        #region ICommand Members

        public string Name { get; private set; }

        #endregion
    }
}
