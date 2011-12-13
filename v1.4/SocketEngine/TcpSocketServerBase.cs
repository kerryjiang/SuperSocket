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
    class TcpSocketServerBase<TSocketSession, TAppSession, TCommandInfo> : SocketServerBase<TSocketSession, TAppSession, TCommandInfo>
        where TAppSession : IAppSession, new()
        where TSocketSession : ISocketSession<TAppSession>
        where TCommandInfo : ICommandInfo
    {
        private byte[] m_KeepAliveOptionValues;

        public TcpSocketServerBase(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ICustomProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint, protocol)
        {
            uint dummy = 0;
            m_KeepAliveOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            //whether enable KeepAlive
            BitConverter.GetBytes((uint)1).CopyTo(m_KeepAliveOptionValues, 0);
            //how long will start first keep alive
            BitConverter.GetBytes((uint)(appServer.Config.KeepAliveTime * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy));
            //keep alive interval
            BitConverter.GetBytes((uint)(appServer.Config.KeepAliveInterval * 1000)).CopyTo(m_KeepAliveOptionValues, Marshal.SizeOf(dummy) * 2);
        }

        protected TSocketSession RegisterSession(Socket client, TSocketSession session)
        {
            //load socket setting
            if (AppServer.Config.ReadTimeOut > 0)
                client.ReceiveTimeout = AppServer.Config.ReadTimeOut;

            if (AppServer.Config.SendTimeOut > 0)
                client.SendTimeout = AppServer.Config.SendTimeOut;

            if (AppServer.Config.ReceiveBufferSize > 0)
                client.ReceiveBufferSize = AppServer.Config.ReceiveBufferSize;

            if (AppServer.Config.SendBufferSize > 0)
                client.SendBufferSize = AppServer.Config.SendBufferSize;

            if (!Platform.SupportSocketIOControlByCodeEnum)
            {
                client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
            else
            {
                client.IOControl(IOControlCode.KeepAliveValues, m_KeepAliveOptionValues, null);
            }

            client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            TAppSession appSession = this.AppServer.CreateAppSession(session);

            if (appSession == null)
                return default(TSocketSession);

            session.Initialize(this.AppServer, appSession);

            return session;
        }
    }
}
