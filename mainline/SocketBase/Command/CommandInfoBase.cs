using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public abstract class CommandInfoBase<TCommandData> : ICommandInfo<TCommandData>
    {
        public CommandInfoBase(string key, TCommandData data)
        {
            CommandKey = key;
            CommandData = data;
        }

        #region ICommandInfo<TCommandData> Members

        public TCommandData CommandData { get; private set; }

        #endregion

        #region ICommandInfo Members

        public string CommandKey { get; private set; }

        #endregion
    }
}
