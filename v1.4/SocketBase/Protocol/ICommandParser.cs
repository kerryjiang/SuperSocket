using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// The interface for command parser 
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// Parses the command.
        /// </summary>
        /// <param name="command">The command line.</param>
        /// <returns>return the parsed StringCommandInfo</returns>
        StringCommandInfo ParseCommand(string command);
    }
}
