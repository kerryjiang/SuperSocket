using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;
using System.Reflection;

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
