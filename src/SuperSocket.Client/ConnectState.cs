using System;
using System.IO;
using System.Net.Sockets;

namespace SuperSocket.Client
{
    public class ConnectState
    {
        public ConnectState()
        {

        }

        private ConnectState(bool cancelled)
        {
            Cancelled = cancelled;
        }

        public bool Result { get; set; }

        public bool Cancelled { get; private set; }

        public Exception Exception { get; set; }

        public Socket Socket { get; set; }

        public Stream Stream { get; set; }

        public static readonly ConnectState CancelledState = new ConnectState(false);
    }
}