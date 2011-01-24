using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandParameterParser
    {
        string[] ParseCommandParameter(string parameter);
    }
}
