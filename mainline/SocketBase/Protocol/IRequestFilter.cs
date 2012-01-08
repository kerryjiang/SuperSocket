using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public interface IRequestFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        TRequestInfo Filter(IAppSession<TRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left);

        int LeftBufferSize { get; }

        IRequestFilter<TRequestInfo> NextRequestFilter { get; }
    }
}
