using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    public class StartResult
    {
        public bool Result { get; set; }

        public string Message { get; set; }

        public ServerInfo ServerInfo { get; set; }
    }
}
