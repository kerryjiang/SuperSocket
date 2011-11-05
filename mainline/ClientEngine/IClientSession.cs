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
        void Initialize(NameValueCollection settings, IClientCommandReader<TCommandInfo> commandReader);

        void Initialize(NameValueCollection settings, IClientCommandReader<TCommandInfo> commandReader, IEnumerable<Assembly> commandAssemblies);

        void RegisterCommandHandler(string name, Action<IClientSession<TCommandInfo, TContext>, TCommandInfo> execution);

        void Connect(IPEndPoint remoteEndPoint);

        void Send(byte[] data, int offset, int length);

        void Close();

        TContext Context { get; set; }
    }
}
