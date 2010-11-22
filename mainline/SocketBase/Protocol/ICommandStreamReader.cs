using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public interface ICommandStreamReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        void InitializeReader(SocketContext context, Stream stream, Encoding encoding, int bufferSize);

        TCommandInfo ReadCommand();
    }
}
