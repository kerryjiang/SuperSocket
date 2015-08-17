using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket
{
    public enum OpCode : sbyte
    {
        Continuation = 0,

        Text = 1,

        Binary = 2,

        Close = 8,

        Ping = 9,

        Pong = 10
    }
}
