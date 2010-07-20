using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public class BasicCommandParser : ICommandParser
    {
        private string m_Spliter;

        public BasicCommandParser() : this(" ")
        {
        }

        public BasicCommandParser(string spliter)
        {
            m_Spliter = spliter;
        }

        #region ICommandParser Members

        public CommandInfo ParseCommand(string command)
        {
            int pos = command.IndexOf(m_Spliter);

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
