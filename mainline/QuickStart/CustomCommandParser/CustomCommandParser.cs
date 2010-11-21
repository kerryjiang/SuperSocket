using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    /// <summary>
    /// CMD:ECHO AabSfght5656D5Cfa5==
    /// </summary>
    public class CustomCommandParser : ICommandParser
    {
        #region ICommandParser Members

        public StringCommandInfo ParseCommand(string command)
        {
            if(!command.StartsWith("CMD:"))
                return null;

            command = command.Substring(4);
            string[] data = command.Split(' ');
            return new StringCommandInfo(data[0], data[1],
                Encoding.ASCII.GetString(Convert.FromBase64String(data[1])).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
