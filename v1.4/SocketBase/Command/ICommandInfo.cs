using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public interface ICommandInfo
    {
        string Key { get; }
    }

    public interface ICommandInfo<TCommandData> : ICommandInfo
    {
        TCommandData Data { get; }
    }
}
