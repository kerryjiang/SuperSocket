using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.ClientEngine
{
    public abstract class ClientSession<TCommandInfo, TContext> : IClientSession<TCommandInfo, TContext>
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        protected Socket Client { get; set; }

        protected IClientCommandReader<TCommandInfo> CommandReader { get; private set; }

        public TContext Context { get; set; }

        protected EndPoint RemoteEndPoint { get; set; }

        private Dictionary<string, ICommand<TCommandInfo, TContext>> m_CommandDict
            = new Dictionary<string, ICommand<TCommandInfo, TContext>>(StringComparer.OrdinalIgnoreCase);

        public ClientSession(IClientCommandReader<TCommandInfo> commandReader)
            : this(commandReader, null, null)
        {

        }

        public ClientSession(IClientCommandReader<TCommandInfo> commandReader, EndPoint remoteEndPoint)
            : this(commandReader, null, remoteEndPoint)
        {

        }

        public ClientSession(IClientCommandReader<TCommandInfo> commandReader, IEnumerable<Assembly> commandAssemblies)
            : this(commandReader, commandAssemblies, null)
        {

        }

        public ClientSession(IClientCommandReader<TCommandInfo> commandReader, IEnumerable<Assembly> commandAssemblies, EndPoint remoteEndPoint)
        {
            if (remoteEndPoint != null)
                RemoteEndPoint = remoteEndPoint;

            CommandReader = commandReader;

            if (commandAssemblies != null)
                SetupAssemblyCommands(commandAssemblies);
        }

        private void SetupAssemblyCommands(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var c in assembly.GetImplementedObjectsByInterface<ICommand<TCommandInfo, TContext>>())
                {
                    m_CommandDict.Add(c.Name, c);
                }
            }
        }

        public void RegisterCommandHandler(string name, Action<IClientSession<TCommandInfo, TContext>, TCommandInfo> execution)
        {
            m_CommandDict.Add(name, new DelegateCommand<TCommandInfo, TContext>(name, execution));
        }

        protected void ExecuteCommand(TCommandInfo commandInfo)
        {
            ICommand<TCommandInfo, TContext> command;

            if (m_CommandDict.TryGetValue(commandInfo.Key, out command))
            {
                command.ExecuteCommand(this, commandInfo);
            }
        }

        void IClientSession.Connect()
        {
            Connect();
        }

        protected abstract void Connect();

        public abstract void Send(byte[] data, int offset, int length);

        public abstract void Close();

        private EventHandler m_Closed;

        public event EventHandler Closed
        {
            add { m_Closed += value; }
            remove { m_Closed -= value; }
        }

        protected void OnClosed()
        {
            var handler = m_Closed;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
