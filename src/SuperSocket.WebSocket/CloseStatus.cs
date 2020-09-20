using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public class CloseStatus
    {
        public CloseReason Reason { get; set; }

        public string ReasonText { get; set; }

        public bool RemoteInitiated{ get; set; }
    }
}
