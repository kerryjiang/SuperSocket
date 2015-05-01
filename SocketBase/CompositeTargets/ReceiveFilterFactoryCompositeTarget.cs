using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase.CompositeTargets
{
    class ReceiveFilterFactoryCompositeTarget<TPackageInfo> : SingleResultCompositeTarget<IReceiveFilterFactory<TPackageInfo>>
        where TPackageInfo : IPackageInfo
    {
        public ReceiveFilterFactoryCompositeTarget(Action<IReceiveFilterFactory<TPackageInfo>> callback)
            : base((config) => config.ReceiveFilterFactory, callback, false)
        {

        }
    }
}
