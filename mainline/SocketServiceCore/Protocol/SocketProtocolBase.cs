using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public abstract class SocketProtocolBase : ICommandParserProtocol
    {
        public virtual ICommandParser CommandParser
        {
            get { return new BasicCommandParser(); }
        }

        public virtual ICommandParameterParser CommandParameterParser
        {
            get { return new SplitAllCommandParameterParser(); }
        }
    }
}
