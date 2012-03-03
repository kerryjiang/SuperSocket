using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.Model
{
    public class InstanceModel
    {
        public ServerModel Server { get; set; }

        public InstanceInfo Instance { get; set; }
    }
}
