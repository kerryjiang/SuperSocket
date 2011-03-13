using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PerformanceTestAgent.ViewModel
{
    public class AsyncUserToken
    {
        public Socket Socket { get; set; }
        public int RequireLength { get; set; }
        public int ReceivedLength { get; set; }
    }
}
