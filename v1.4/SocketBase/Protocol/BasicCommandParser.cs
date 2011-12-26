using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// It is a command parser for the command whose command name and command parameters are separated by specific char(s)
    /// </summary>
    public class BasicCommandParser : ICommandParser
    {
        private string m_Spliter;
        private string[] m_ParameterSpliters;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicCommandParser"/> class.
        /// </summary>
        public BasicCommandParser() : this(" ", " ")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicCommandParser"/> class.
        /// </summary>
        /// <param name="spliter">The spliter between command name and command parameters.</param>
        /// <param name="parameterSpliter">The parameter spliter.</param>
        public BasicCommandParser(string spliter, string parameterSpliter)
        {
            m_Spliter = spliter;
            m_ParameterSpliters = new string[] { parameterSpliter };
        }

        #region ICommandParser Members

        /// <summary>
        /// Parses the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
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
                param.Split(m_ParameterSpliters, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
