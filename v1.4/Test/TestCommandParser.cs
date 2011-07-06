using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test
{
    public class TestCommandParser : ICommandParser
    {
        #region ICommandParser Members

        public StringCommandInfo ParseCommand(string command)
        {
            int pos = command.IndexOf(':');

            if(pos <= 0)
                return null;

            string param = command.Substring(pos + 1);

            return new StringCommandInfo(command.Substring(0, pos), param,
                param.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
