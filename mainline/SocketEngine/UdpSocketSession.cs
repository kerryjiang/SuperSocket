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

        private ICommandAsyncReader<TCommandInfo> m_CommandReader;

        private SocketContext m_Context;

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, ICommandAsyncReader<TCommandInfo> commandReader)
            : base()
        {
            m_ServerSocket = serverSocket;
            m_RemoteEndPoint = remoteEndPoint;
            IdentityKey = m_RemoteEndPoint.ToString();
            m_CommandReader = commandReader;
        }

        public override IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_ServerSocket.LocalEndPoint; }
        }

        private readonly IPEndPoint m_RemoteEndPoint;

        public override IPEndPoint RemoteEndPoint
        {
            get { return m_RemoteEndPoint; }
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
            var commandInfo = m_CommandReader.FindCommand(data, offset, length);

            m_CommandReader = m_CommandReader.NextCommandReader;

            if (commandInfo == null)
                return;

            ExecuteCommand(commandInfo);
        }

        public override void SendResponse(SocketContext context, string message)
        {
            byte[] data = context.Charset.GetBytes(message);
            m_ServerSocket.SendTo(data, m_RemoteEndPoint);
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
