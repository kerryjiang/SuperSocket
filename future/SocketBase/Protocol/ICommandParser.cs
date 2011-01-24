using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandParser
    {
        StringCommandInfo ParseCommand(string command);
    }
}
