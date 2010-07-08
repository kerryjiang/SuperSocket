using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class SplitAllCommandParameterParser : ICommandParameterParser
    {
        #region ICommandParameterParser Members

        public void ParseCommandParameter(CommandInfo command)
        {
            string[] arrParam = command.Param.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            command.InitializeParameters(arrParam);
        }

        #endregion
    }
}
