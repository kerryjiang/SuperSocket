using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Client.ViewModel
{
    public class FaultInstanceViewModel : InstanceViewModelBase
    {
        public FaultInstanceViewModel(ServerViewModel server)
            : base(server)
        {
            FaultDescription = server.Description;
        }

        public string FaultDescription { get; private set; }
    }
}
