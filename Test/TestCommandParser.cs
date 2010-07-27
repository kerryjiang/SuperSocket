using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test
{
    public class TestCommandParser : ICommandParser
    {
        #region ICommandParser Members

        public CommandInfo ParseCommand(string command)
        {
            int pos = command.IndexOf(':');

            if(pos <= 0)
                return null;

            return new CommandInfo(command.Substring(0, pos), command.Substring(pos + 1));
        }

        #endregion
    }
}
