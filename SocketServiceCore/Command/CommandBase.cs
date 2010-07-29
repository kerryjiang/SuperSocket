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
            if (m_CommandParameterParser != null)           
                commandData.InitializeParameters(m_CommandParameterParser.ParseCommandParameter(commandData.Param));
            else
                commandData.InitializeParameters(DefaultParameterParser.ParseCommandParameter(commandData.Param));
            //Excute command
            Execute(session, commandData);
        }

        public ICommandParameterParser DefaultParameterParser { get; set; }

        #endregion

        protected abstract void Execute(T session, CommandInfo commandData);
    }
}
