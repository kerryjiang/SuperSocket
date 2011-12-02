using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ClientEngine
{
    class DelegateCommand<TCommandInfo, TContext> : ICommand<TCommandInfo, TContext>
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        private Action<IClientSession<TCommandInfo, TContext>, TCommandInfo> m_Execution;

        public DelegateCommand(string name, Action<IClientSession<TCommandInfo, TContext>, TCommandInfo> execution)
        {
            Name = name;
            m_Execution = execution;
        }

        public void ExecuteCommand(IClientSession<TCommandInfo, TContext> session, TCommandInfo commandInfo)
        {
            m_Execution(session, commandInfo);
        }

        public string Name { get; private set; }
    }
}
