using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public interface ICommandParserProtocol
    {
        ICommandParser CommandParser { get; }

        ICommandParameterParser CommandParameterParser { get; }
    }
}
