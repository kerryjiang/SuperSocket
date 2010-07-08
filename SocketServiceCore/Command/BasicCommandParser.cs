using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class BasicCommandParser : ICommandParser
    {
        #region ICommandParser Members

        public CommandInfo ParseCommand(string command)
        {
            int pos = command.IndexOf(' ');

            string name = string.Empty;
            string param = string.Empty;

            if (pos > 0)
            {
                name = command.Substring(0, pos);
                param = command.Substring(pos + 1);
            }
            else
            {
                name = command;
            }

            return new CommandInfo(name, param);
        }

        #endregion
    }
}
