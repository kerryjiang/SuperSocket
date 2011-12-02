using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public interface IClientCommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        TCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left);
    }
}
