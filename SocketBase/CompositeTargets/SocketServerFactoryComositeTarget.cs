using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase.CompositeTargets
{
    class SocketServerFactoryComositeTarget : SingleResultCompositeTarget<ISocketServerFactory>
    {
        public SocketServerFactoryComositeTarget(Action<ISocketServerFactory> callback)
            : base((config) => config.Options.GetValue("socketServerFactory"), callback, true)
        {

        }
    }
}
