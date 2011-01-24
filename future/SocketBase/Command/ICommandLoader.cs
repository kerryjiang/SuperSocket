using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandLoader<TCommandInfo>
        where TCommandInfo : class
    {
        IEnumerable<TCommandInfo> LoadCommands();
    }
}
