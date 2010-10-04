using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    class Base64CommandParameterParser : ICommandParameterParser
    {
        #region ICommandParameterParser Members

        public string[] ParseCommandParameter(string parameter)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(parameter)).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
