using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Protocol
{
    public interface ICommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        IAppServer AppServer { get; }

        TCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left);

        byte[] GetLeftBuffer();

        int LeftBufferSize { get; }

        ICommandReader<TCommandInfo> NextCommandReader { get; }
    }
}
