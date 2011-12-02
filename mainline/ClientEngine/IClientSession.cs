using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public interface IClientSession
    {
        void Connect();

        void Send(byte[] data, int offset, int length);

        void Close();
    }

    public interface IClientSession<TCommandInfo, TContext> : IClientSession
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        void RegisterCommandHandler(string name, Action<IClientSession<TCommandInfo, TContext>, TCommandInfo> execution);

        TContext Context { get; set; }
    }
}
