using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace SuperSocket.Management.Client.ViewModel
{
    public abstract class InstanceViewModelBase : MyViewModelBase
    {
        public InstanceViewModelBase(ServerViewModel server)
        {
            Server = server;
        }

        public ServerViewModel Server { get; private set; }

        public Type SelfType
        {
            get { return this.GetType(); }
        }
    }
}
