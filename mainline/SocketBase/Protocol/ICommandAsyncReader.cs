using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public interface ICommandAsyncReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        TCommandInfo FindCommand(SocketContext context, byte[] readBuffer, int offset, int length, bool isReusableBuffer);

        ArraySegmentList<byte> GetLeftBuffer();

        ICommandAsyncReader<TCommandInfo> NextCommandReader { get; }
    }
}
