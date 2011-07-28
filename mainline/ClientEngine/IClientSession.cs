using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;

namespace SuperSocket.ClientEngine
{
    public interface IClientSession<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        void Initialize(IClientCommandReader<TCommandInfo> commandReader, NameValueCollection settings);

        void Connect(IPEndPoint remoteEndPoint);

        void Send(byte[] data, int offset, int length);

        void Close();
    }
}
