using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandInfo
    {
        string CommandKey { get; }
    }

    public interface ICommandInfo<TCommandData> : ICommandInfo
    {        
        TCommandData CommandData { get; }
    }
}
