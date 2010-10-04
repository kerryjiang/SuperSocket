using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    enum SearhMarkStatus
    {
        None,
        Found,
        FoundStart
    }

    class SearhMarkResult
    {
        public SearhMarkStatus Status { get; set; }
        public int Value { get; set; }
    }
}
