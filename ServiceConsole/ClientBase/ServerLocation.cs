using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ServiceConsole.ClientBase
{
    public class ServerLocation
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public ServerLocationStatus Status { get; set; }
    }

    public enum ServerLocationStatus
    {
        Connected,
        Disconnected
    }
}
