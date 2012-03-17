using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace SuperSocket.Management.Client.ViewModel
{
    public  abstract class InstanceViewModelBase : ViewModelBase
    {
        public InstanceViewModelBase(ServerViewModel server)
        {
            Server = server;
        }

        public ServerViewModel Server { get; private set; }
    }
}
