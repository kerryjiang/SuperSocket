using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.CompositeTargets
{
    class CommandLoaderCompositeTarget<TCommand> : ICompositeTarget
        where TCommand : class, ICommand
    {
        private Action<List<ICommandLoader<TCommand>>> m_Callback;

        public CommandLoaderCompositeTarget(Action<List<ICommandLoader<TCommand>>> callback)
        {
            m_Callback = callback;
        }

        public bool Resolve(IAppServer appServer, ExportProvider exportProvider)
        {
            m_Callback(new List<ICommandLoader<TCommand>>());
            return true;
        }
    }
}
