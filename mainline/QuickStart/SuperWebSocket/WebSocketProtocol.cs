using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.Protocol;
using SuperSocket.SocketServiceCore.Protocol;

namespace SuperWebSocket
{
    public class WebSocketProtocol : SocketProtocolBase, IAsyncProtocol
    {
        #region IAsyncProtocol Members

        public ICommandAsyncReader CreateAsyncCommandReader()
        {
            return new HeaderAsyncReader();
        }

        #endregion
    }
}
