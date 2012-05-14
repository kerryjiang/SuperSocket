using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    abstract class TcpSocketServerBase : SocketServerBase
    {
        private readonly byte[] m_KeepAliveOptionValues;
        private readonly int m_SendTimeOut;
        private readonly int m_ReceiveBufferSize;
        private readonly int m_SendBufferSize;

        public TcpSocketServerBase(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
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

            m_SendTimeOut = config.SendTimeOut;
            m_ReceiveBufferSize = config.ReceiveBufferSize;
            m_SendBufferSize = config.SendBufferSize;
        }

        protected ISocketSession RegisterSession(Socket client, ISocketSession session)
        {
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

            client.NoDelay = true;
            client.UseOnlyOverlappedIO = true;
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            IAppSession appSession = this.AppServer.CreateAppSession(session);

            if (appSession == null)
                return null;

            session.Initialize(appSession);

            return session;
        }

        public override bool Start()
        {
            if (!base.Start())
                return false;

            ILog log = AppServer.Logger;

            for (var i = 0; i < ListenerInfos.Length; i++)
            {
                var listener = CreateListener(ListenerInfos[i]);
                listener.Error += new ErrorHandler(listener_Error);
                listener.NewClientAccepted += new NewClientAcceptHandler(AcceptNewClient);

                if (listener.Start())
                {
                    Listeners.Add(listener);

                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Listener ({0}) was started", listener.EndPoint);
                    }
                }
                else //If one listener failed to start, stop started listeners
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Listener ({0}) failed to start", listener.EndPoint);
                    }

                    for (var j = 0; j < Listeners.Count; j++)
                    {
                        Listeners[j].Stop();
                    }

                    Listeners.Clear();
                    return false;
                }
            }

            return true;
        }

        protected abstract void AcceptNewClient(ISocketListener listener, Socket client);

        void listener_Error(ISocketListener listener, Exception e)
        {
            this.AppServer.Logger.Error(string.Format("Listener ({0}) error", listener.EndPoint), e);
        }

        protected virtual ISocketListener CreateListener(ListenerInfo listenerInfo)
        {
            return new TcpAsyncSocketListener(listenerInfo);
        }

        public override void Stop()
        {
            base.Stop();

            for (var i = 0; i < Listeners.Count; i++)
            {
                Listeners[i].Stop();
            }

            Listeners.Clear();
        }
    }
}
