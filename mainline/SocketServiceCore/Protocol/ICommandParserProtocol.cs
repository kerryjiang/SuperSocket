using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public interface ICommandParserProtocol
    {
        ICommandParser CommandParser { get; }

        ICommandParameterParser CommandParameterParser { get; }
    }
}
