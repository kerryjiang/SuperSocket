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
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    class TcpSocketServerBase<TSocketSession, TAppSession, TRequestInfo> : SocketServerBase<TSocketSession, TAppSession, TRequestInfo>
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>
        where TRequestInfo : IRequestInfo
    {
        private readonly byte[] m_KeepAliveOptionValues;
        private readonly int m_ReadTimeOut;
        private readonly int m_SendTimeOut;
        private readonly int m_ReceiveBufferSize;
        private readonly int m_SendBufferSize;

        public TcpSocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, IRequestFilterFactory<TRequestInfo> requestFilterFactory)
            : base(appServer, localEndPoint, requestFilterFactory)
        {
            var config = appServer.Config;

            uint dummy = 0;
            m_KeepAliveOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            //whether enable KeepAlive
            BitConverter.GetBytes((uint)1).CopyTo(m_KeepAliveOptionValues, 0);
            //how long will start first keep alive
            BitConverter.GetBytes((uint)(config.KeepAliveTime * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy));
            //keep alive interval
            BitConverter.GetBytes((uint)(config.KeepAliveInterval * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy) * 2);

            m_ReadTimeOut = config.ReadTimeOut;
            m_SendTimeOut = config.SendTimeOut;
            m_ReceiveBufferSize = config.ReceiveBufferSize;
            m_SendBufferSize = config.SendBufferSize;
        }

        protected TSocketSession RegisterSession(Socket client, TSocketSession session)
        {
            //load socket setting
            if (m_ReadTimeOut > 0)
                client.ReceiveTimeout = m_ReadTimeOut;

            if (m_SendTimeOut > 0)
                client.SendTimeout = m_SendTimeOut;

            if (m_ReceiveBufferSize > 0)
                client.ReceiveBufferSize = m_ReceiveBufferSize;

            if (m_SendBufferSize > 0)
                client.SendBufferSize = m_SendBufferSize;

            if(!Platform.SupportSocketIOControlByCodeEnum)
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            else
                client.IOControl(IOControlCode.KeepAliveValues, m_KeepAliveOptionValues, null);

            client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            client.DontFragment = false;
            client.UseOnlyOverlappedIO = true;

            TAppSession appSession = this.AppServer.CreateAppSession(session);

            if (appSession == null)
                return default(TSocketSession);

            session.Initialize(this.AppServer, appSession);

            return session;
        }
    }
}
