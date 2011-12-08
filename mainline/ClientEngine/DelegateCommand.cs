using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ClientEngine
{
    class DelegateCommand<TClientSession, TCommandInfo> : ICommand<TClientSession, TCommandInfo>
        where TClientSession : IClientSession
        where TCommandInfo : ICommandInfo
    {
        private Action<TClientSession, TCommandInfo> m_Execution;

        public DelegateCommand(string name, Action<TClientSession, TCommandInfo> execution)
        {
            Name = name;
            m_Execution = execution;
        }

        public void ExecuteCommand(TClientSession session, TCommandInfo commandInfo)
        {
            m_Execution(session, commandInfo);
        }

        public string Name { get; private set; }
    }
}
