using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Shared
{
    public class InstanceInfo
    {
        public string Name { get; set; }

        public bool IsRunning { get; set; }

        public DateTime StartedTime { get; set; }

        public int MaxConnectionCount { get; set; }

        public int CurrentConnectionCount { get; set; }

        public string Listener { get; set; }

        public int RequestHandlingSpeed { get; set; }
    }
}
