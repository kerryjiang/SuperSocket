using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Metadata;
using SuperSocket.ProtoBase;

namespace SuperSocket.Dlr
{
    class DynamicCommand<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>, ICommandFilterProvider
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IPackageInfo
    {
        private Action<TAppSession, TRequestInfo> m_DynamicExecuteCommand;

        private IEnumerable<CommandFilterAttribute> m_Filters;

        public DynamicCommand(ScriptRuntime scriptRuntime, IScriptSource source)
        {
            Source = source;

            Name = source.Name;

            var scriptEngine = scriptRuntime.GetEngineByFileExtension(source.LanguageExtension);
            var scriptScope = scriptEngine.CreateScope();

            var scriptSource = scriptEngine.CreateScriptSourceFromString(source.GetScriptCode(), SourceCodeKind.File);
            var compiledCode = scriptSource.Compile();
            compiledCode.Execute(scriptScope);

            Action<TAppSession, TRequestInfo> dynamicMethod;
            if (!scriptScope.TryGetVariable<Action<TAppSession, TRequestInfo>>("execute", out dynamicMethod))
                throw new Exception("Failed to find a command execution method in source: " + source.Tag);

            Func<IEnumerable<CommandFilterAttribute>> filtersAction;

            if (scriptScope.TryGetVariable<Func<IEnumerable<CommandFilterAttribute>>>("getFilters", out filtersAction))
                m_Filters = filtersAction();

            CompiledTime = DateTime.Now;

            m_DynamicExecuteCommand = dynamicMethod;
        }

        #region ICommand<TAppSession,TRequestInfo> Members

        public virtual void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
        {
            m_DynamicExecuteCommand(session, requestInfo);
        }

        #endregion

        public IScriptSource Source { get; private set; }

        public DateTime CompiledTime { get; private set; }

        #region ICommand Members

        public string Name { get; private set; }

        #endregion

        public override string ToString()
        {
            return Source.Tag;
        }

        public IEnumerable<CommandFilterAttribute> GetFilters()
        {
            return m_Filters;
        }
    }
}
