using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    class Rfc6455ReceiveFilter : DraftHybi10ReceiveFilter, IHandshakeHandler
    {
        public Rfc6455ReceiveFilter()
            : base()
        {

        }
    }
}
