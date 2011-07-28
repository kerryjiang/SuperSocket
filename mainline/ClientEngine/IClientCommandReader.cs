using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.ClientEngine
{
    public interface IClientCommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        TCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left);
    }
}
