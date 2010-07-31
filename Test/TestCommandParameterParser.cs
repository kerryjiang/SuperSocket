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

        public string[] ParseCommandParameter(string parameter)
        {
            Console.WriteLine("Parameter line: " + parameter);
            return parameter.Split(',');
        }

        #endregion
    }
}
