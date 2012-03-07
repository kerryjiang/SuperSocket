using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.ViewModel
{
    public class InstanceRowViewModel
    {
        public ServerViewModel Server { get; set; }

        public InstanceViewModel Instance { get; set; }
    }
}
