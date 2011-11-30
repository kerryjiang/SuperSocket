using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    interface IAsyncSocketSession
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }
        void ProcessReceive(SocketAsyncEventArgs e);
        ILogger Logger { get; }
    }

    class AsyncSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>, IAsyncSocketSession
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public AsyncSocketSession(Socket client, ICommandReader<TCommandInfo> initialCommandReader)
            : base(client, initialCommandReader)
        {

        }

        ILogger IAsyncSocketSession.Logger
        {
            get { return AppServer.Logger; }
        }

        public override void Start()
        {
            SocketAsyncProxy.Initialize(Client, this);
            StartSession();
            StartReceive(SocketAsyncProxy.SocketEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            if (IsClosed)
                return;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        public override void SendResponse(string message)
        {
            if (IsClosed)
                return;

            byte[] data = AppSession.Charset.GetBytes(message);

            if (IsClosed)
                return;

            try
            {
                Client.SendData(data);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }

        public override void SendResponse(byte[] data)
        {
            SendResponse(data, 0, data.Length);
        }

        public override void SendResponse(byte[] data, int offset, int length)
        {
            if (IsClosed)
                return;

            try
            {
                Client.SendData(data, offset, length);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            if (e.BytesTransferred <= 0)
            {
                Close(CloseReason.ClientClosing);
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Close(CloseReason.SocketError);
                return;
            }

            int bytesTransferred = e.BytesTransferred;
            int offset = e.Offset;

            while (bytesTransferred > 0)
            {
                int left;

                TCommandInfo commandInfo = FindCommand(e.Buffer, offset, bytesTransferred, true, out left);
                
                if (IsClosed)
                    return;

                if (commandInfo == null)
                    break;

                try
                {
                    ExecuteCommand(commandInfo);
                }
                catch (Exception exc)
                {
                    AppServer.Logger.LogError(this, exc);
                    HandleExceptionalError(exc);
                }

                if (left <= 0)
                    break;

                bytesTransferred = left;
                offset = e.Offset + e.BytesTransferred - left;
            }

            //read the next block of data sent from the client
            StartReceive(e);
        }      

        public override void ApplySecureProtocol()
        {
            //TODO: Implement async socket SSL/TLS encryption
        }
    }
}
