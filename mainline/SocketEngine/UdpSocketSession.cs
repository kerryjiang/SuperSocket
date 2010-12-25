using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketEngine
{
    class UdpSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        private Socket m_ServerSocket;

        private ICommandReader<TCommandInfo> m_CommandReader;

        private SocketContext m_Context;

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, ICommandReader<TCommandInfo> commandReader)
            : base()
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
            IdentityKey = remoteEndPoint.ToString();
            m_CommandReader = commandReader;
        }

        public override IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_ServerSocket.LocalEndPoint; }
        }

        protected override void Start(SocketContext context)
        {
            m_Context = context;
        }

        internal void ProcessData(byte[] data)
        {
            ProcessData(data, 0, data.Length);
        }

        internal void ProcessData(byte[] data, int offset, int length)
        {
            TCommandInfo commandInfo;

            try
            {
                commandInfo = m_CommandReader.FindCommand(m_Context, data, offset, length, false);
            }
            catch (ExceedMaxCommandLengthException exc)
            {
                AppServer.Logger.LogError(this, string.Format("Max command length: {0}, current processed length: {1}",
                    exc.MaxCommandLength, exc.CurrentProcessedLength));
                Close(CloseReason.ServerClosing);
                return;
            }
            catch (Exception exce)
            {
                AppServer.Logger.LogError(this, exce);
                Close(CloseReason.ServerClosing);
                return;
            }

            m_CommandReader = m_CommandReader.NextCommandReader;

            if (commandInfo == null)
                return;

            ExecuteCommand(commandInfo);
        }

        public override void SendResponse(SocketContext context, string message)
        {
            byte[] data = context.Charset.GetBytes(message);
            m_ServerSocket.SendTo(data, RemoteEndPoint);
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            throw new NotSupportedException();
        }

        public override void ApplySecureProtocol(SocketContext context)
        {
            throw new NotSupportedException();
        }

        public override void ReceiveData(Stream storeSteram, int length)
        {
            throw new NotSupportedException();
        }

        public override void ReceiveData(Stream storeSteram, byte[] endMark)
        {
            throw new NotSupportedException();
        }
    }
}
