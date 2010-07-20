using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public abstract class CommandBase<T> : ICommand<T> where T : IAppSession
    {
        private readonly ICommandParameterParser m_CommandParameterParser;

        public CommandBase()
            : this(new SplitAllCommandParameterParser())
        {

        }

        public CommandBase(string spliter)
            : this(new SplitAllCommandParameterParser(spliter))
        {

        }

        public CommandBase(ICommandParameterParser commandParameterParser)
        {
            m_CommandParameterParser = commandParameterParser;
        }

        #region ICommand<T> Members

        public void ExecuteCommand(T session, CommandInfo commandData)
        {
            //Prepare parameters
            commandData.InitializeParameters(m_CommandParameterParser.ParseCommandParameter(commandData));
            //Excute command
            Execute(session, commandData);
        }

        #endregion

        protected abstract void Execute(T session, CommandInfo commandData);
    }
}
