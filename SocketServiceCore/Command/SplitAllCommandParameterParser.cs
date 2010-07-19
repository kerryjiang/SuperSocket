using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class SplitAllCommandParameterParser : ICommandParameterParser
    {
        #region ICommandParameterParser Members

        public string[] ParseCommandParameter(CommandInfo command)
        {
            return command.Param.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
