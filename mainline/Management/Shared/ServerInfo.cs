using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    public class ServerInfo
    {
        public string Name { get; set; }

        public bool IsRunning { get; set; }

        public DateTime StartedTime { get; set; }

        public TimeSpan RunningTime { get; set; }

        public int MaxConnectionCount { get; set; }

        public int CurrentConnectionCount { get; set; }

        public ListenerInfo[] Listeners { get; set; }
    }
}
