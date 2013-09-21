using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    abstract class TcpSocketServerBase : SocketServerBase
    {
        private readonly byte[] m_KeepAliveOptionValues;
        private readonly byte[] m_KeepAliveOptionOutValues;
        private readonly int m_SendTimeOut;
        private readonly int m_ReceiveBufferSize;
        private readonly int m_SendBufferSize;

        public TcpSocketServerBase(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {
            var config = appServer.Config;

            uint dummy = 0;
            m_KeepAliveOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            m_KeepAliveOptionOutValues = new byte[m_KeepAliveOptionValues.Length];
            //whether enable KeepAlive
            BitConverter.GetBytes((uint)1).CopyTo(m_KeepAliveOptionValues, 0);
            //how long will start first keep alive
            BitConverter.GetBytes((uint)(config.KeepAliveTime * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy));
            //keep alive interval
            BitConverter.GetBytes((uint)(config.KeepAliveInterval * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy) * 2);

            m_SendTimeOut = config.SendTimeOut;
            m_ReceiveBufferSize = config.ReceiveBufferSize;
            m_SendBufferSize = config.SendBufferSize;
        }

        protected IAppSession CreateSession(Socket client, ISocketSession session)
        {
            if (m_SendTimeOut > 0)
                client.SendTimeout = m_SendTimeOut;

            if (m_ReceiveBufferSize > 0)
                client.ReceiveBufferSize = m_ReceiveBufferSize;

            if (m_SendBufferSize > 0)
                client.SendBufferSize = m_SendBufferSize;

            if (!Platform.SupportSocketIOControlByCodeEnum)
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, m_KeepAliveOptionValues);
            else
                client.IOControl(IOControlCode.KeepAliveValues, m_KeepAliveOptionValues, m_KeepAliveOptionOutValues);

            client.NoDelay = true;
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            return this.AppServer.CreateAppSession(session);
        }

        protected override ISocketListener CreateListener(ListenerInfo listenerInfo)
        {
            return new TcpAsyncSocketListener(listenerInfo);
        }
    }
}
