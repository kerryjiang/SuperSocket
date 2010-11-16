using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore.Protocol
{
    public interface ICommandAsyncReader
    {
        bool FindCommand(byte[] readBuffer, int offset, int length, out byte[] commandData);

        ArraySegmentList<Byte> GetLeftBuffer();

        ICommandAsyncReader NextCommandReader { get; }
    }
}
