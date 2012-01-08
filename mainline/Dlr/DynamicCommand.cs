using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Dlr
{
    class DynamicCommand<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        private Action<TAppSession, TRequestInfo> m_DynamicExecuteCommand;

        public DynamicCommand(ScriptRuntime scriptRuntime, string filePath, DateTime lastUpdatedTime)
        {
            FilePath = filePath;
            LastUpdatedTime = lastUpdatedTime;

            Name = Path.GetFileNameWithoutExtension(filePath);
            var scriptEngine = scriptRuntime.GetEngineByFileExtension(Path.GetExtension(filePath));
            var scriptScope = scriptEngine.CreateScope();

            var scriptSource = scriptEngine.CreateScriptSourceFromFile(filePath);
            var compiledCode = scriptSource.Compile();
            compiledCode.Execute(scriptScope);

            Action<TAppSession, TRequestInfo> dynamicMethod;
            if (!scriptScope.TryGetVariable<Action<TAppSession, TRequestInfo>>("execute", out dynamicMethod))
                throw new Exception("Failed to find a command execution method in file: " + filePath);

            m_DynamicExecuteCommand = dynamicMethod;
        }

        #region ICommand<TAppSession,TRequestInfo> Members

        public virtual void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
        {
            m_DynamicExecuteCommand(session, requestInfo);
        }

        #endregion

        public string FilePath { get; private set; }

        public DateTime LastUpdatedTime { get; private set; }

        #region ICommand Members

        public string Name { get; private set; }

        #endregion
    }
}
