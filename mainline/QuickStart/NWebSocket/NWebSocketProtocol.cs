using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NWebSocket.Protocol;
using SuperSocket.SocketServiceCore.Protocol;

namespace NWebSocket
{
    public class NWebSocketProtocol : SocketProtocolBase, IAsyncProtocol
    {
        #region IAsyncProtocol Members

        public ICommandAsyncReader CreateAsyncCommandReader()
        {
            return new HeaderAsyncReader();
        }

        #endregion
    }
}
