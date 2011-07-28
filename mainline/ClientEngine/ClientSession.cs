using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.ClientEngine
{
    abstract class ClientSession<TCommandInfo> : IClientSession<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        protected Socket Client { get; set; }

        protected IClientCommandReader<TCommandInfo> CommandReader { get; private set; }

        public virtual void Initialize(IClientCommandReader<TCommandInfo> commandReader, NameValueCollection settings)
        {
            CommandReader = commandReader;
        }

        public abstract void Connect(IPEndPoint remoteEndPoint);

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
