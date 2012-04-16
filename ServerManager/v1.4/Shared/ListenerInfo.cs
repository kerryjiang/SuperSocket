using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SuperSocket.Management.Shared
{
    public class ListenerInfo
    {
        public string EndPoint { get; set; }

        public int BackLog { get; set; }

        public string Security { get; set; }
    }
}
