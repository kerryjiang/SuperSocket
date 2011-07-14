using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public enum CommandUpdateAction
    {
        Add,
        Remove,
        Update
    }

    public class CommandUpdateInfo<T>
    {
        public CommandUpdateAction UpdateAction { get; set; }
        public T Command { get; set; }
    }
}