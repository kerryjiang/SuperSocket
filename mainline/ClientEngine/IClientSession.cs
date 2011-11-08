using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.ClientEngine
{
    public interface IClientSession<TCommandInfo, TContext>
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        void RegisterCommandHandler(string name, Action<IClientSession<TCommandInfo, TContext>, TCommandInfo> execution);

        void Connect();

        void Send(byte[] data, int offset, int length);

        void Close();

        TContext Context { get; set; }
    }
}
