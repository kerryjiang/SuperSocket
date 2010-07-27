using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test
{
    public class TestCommandParameterParser : ICommandParameterParser
    {
        #region ICommandParameterParser Members

        public string[] ParseCommandParameter(CommandInfo command)
        {
            return command.Param.Split(',');
        }

        #endregion
    }
}
