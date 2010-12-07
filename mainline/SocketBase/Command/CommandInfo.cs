using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public abstract class CommandInfo<TCommandData> : ICommandInfo<TCommandData>
    {
        public CommandInfo(string key, TCommandData data)
        {
            Key = key;
            Data = data;
        }

        #region ICommandInfo<TCommandData> Members

        public TCommandData Data { get; private set; }

        #endregion

        #region ICommandInfo Members

        public string Key { get; private set; }

        #endregion
    }
}
