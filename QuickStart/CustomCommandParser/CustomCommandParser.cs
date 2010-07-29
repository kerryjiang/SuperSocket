using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    /// <summary>
    /// CMD:ECHO AabSfght5656D5Cfa5==
    /// </summary>
    public class CustomCommandParser : ICommandParser
    {
        #region ICommandParser Members

        public CommandInfo ParseCommand(string command)
        {
            if(!command.StartsWith("CMD:"))
                return null;

            command = command.Substring(4);
            string[] data = command.Split(' ');
            return new CommandInfo(data[0], data[1]);
        }

        #endregion
    }
}
