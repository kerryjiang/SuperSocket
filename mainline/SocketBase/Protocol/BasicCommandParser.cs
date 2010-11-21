using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public class BasicCommandParser : ICommandParser
    {
        private string m_Spliter;
        private string m_ParameterSpliter;

        public BasicCommandParser() : this(" ", " ")
        {
        }

        public BasicCommandParser(string spliter, string parameterSpliter)
        {
            m_Spliter = spliter;
            m_ParameterSpliter = parameterSpliter;
        }

        #region ICommandParser Members

        public StringCommandInfo ParseCommand(string command)
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

            return new StringCommandInfo(name, param,
                param.Split(new string[] { m_ParameterSpliter }, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
