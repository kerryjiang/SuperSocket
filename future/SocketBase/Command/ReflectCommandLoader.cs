using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SuperSocket.Common;

namespace SuperSocket.SocketBase.Command
{
    public class ReflectCommandLoader<TCommandInfo> : ICommandLoader<TCommandInfo>
        where TCommandInfo : class
    {
        private Assembly m_CommandAssembly;

        public ReflectCommandLoader(Assembly assembly)
        {
            m_CommandAssembly = assembly;
        }

        #region ICommandLoader Members

        public IEnumerable<TCommandInfo> LoadCommands()            
        {
            return m_CommandAssembly.GetImplementedObjectsByInterface<TCommandInfo>();
        }

        #endregion
    }
}
