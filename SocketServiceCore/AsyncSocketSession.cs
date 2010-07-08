using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.AsyncSocket;

namespace SuperSocket.SocketServiceCore
{
    public class AsyncSocketSession<T> : SocketSession<T>, IAsyncSocketSession
        where T : IAppSession, new()
	{
        AutoResetEvent m_SendReceiveResetEvent = new AutoResetEvent(true);

		protected override void Start(SocketContext context)
		{
			Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            SocketAsyncProxy.Initialize(Client, this, context);
			SayWelcome();
            StartReceive();
		}

        private void StartReceive()
        {
            if (Client == null || !Client.Connected)
                return;

            m_SendReceiveResetEvent.WaitOne();

            bool willRaiseEvent = Client.ReceiveAsync(this.SocketAsyncProxy.SocketEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(this.SocketAsyncProxy.SocketEventArgs);
            }
        }

        public override void SendResponse(SocketContext context, string message)
        {
            m_SendReceiveResetEvent.WaitOne();

            if (string.IsNullOrEmpty(message))
                return;

            if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

            AsyncUserToken token = this.SocketAsyncProxy.SocketEventArgs.UserToken as AsyncUserToken;
            token.SendBuffer = context.Charset.GetBytes(message);
            token.Offset = 0;
            if (this.SocketAsyncProxy.SocketEventArgs.Buffer.Length >= token.SendBuffer.Length)
            {
                Buffer.BlockCopy(token.SendBuffer, 0, this.SocketAsyncProxy.SocketEventArgs.Buffer, 0, token.SendBuffer.Length);
                this.SocketAsyncProxy.SocketEventArgs.SetBuffer(0, token.SendBuffer.Length);
                token.SendBuffer = new byte[0];
            }
            else
            {
                Buffer.BlockCopy(token.SendBuffer, 0, this.SocketAsyncProxy.SocketEventArgs.Buffer, 0, this.SocketAsyncProxy.SocketEventArgs.Buffer.Length);
                this.SocketAsyncProxy.SocketEventArgs.SetBuffer(0, this.SocketAsyncProxy.SocketEventArgs.Buffer.Length);
                token.Offset = token.Offset + this.SocketAsyncProxy.SocketEventArgs.Buffer.Length;
            }

            if (!Client.SendAsync(this.SocketAsyncProxy.SocketEventArgs))
                ProcessSend(this.SocketAsyncProxy.SocketEventArgs);
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                string commandLine = token.SocketContext.Charset.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                CommandInfo cmdInfo = new CommandInfo(commandLine.Trim());

                //this round receive has been finished, the buffer can be used for sending
                m_SendReceiveResetEvent.Set();

                ExecuteCommand(cmdInfo);
                //read the next block of data send from the client
                StartReceive();
            }
            else
            {
                Close();
            }
        }

        public void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken)e.UserToken;

                //continue send
                if (token.SendBuffer.Length != token.Offset)
                {
                    int leftBytes = token.SendBuffer.Length - token.Offset;

                    if (this.SocketAsyncProxy.SocketEventArgs.Buffer.Length >= leftBytes)
                    {
                        Buffer.BlockCopy(token.SendBuffer, token.Offset, this.SocketAsyncProxy.SocketEventArgs.Buffer, 0, token.SendBuffer.Length);
                        this.SocketAsyncProxy.SocketEventArgs.SetBuffer(0, token.SendBuffer.Length);
                        token.SendBuffer = new byte[0];
                    }
                    else
                    {
                        Buffer.BlockCopy(token.SendBuffer, token.Offset, this.SocketAsyncProxy.SocketEventArgs.Buffer, 0, this.SocketAsyncProxy.SocketEventArgs.Buffer.Length);
                        this.SocketAsyncProxy.SocketEventArgs.SetBuffer(0, this.SocketAsyncProxy.SocketEventArgs.Buffer.Length);
                        token.Offset = token.Offset + this.SocketAsyncProxy.SocketEventArgs.Buffer.Length;
                    }

                    if (!Client.SendAsync(this.SocketAsyncProxy.SocketEventArgs))
                        ProcessSend(this.SocketAsyncProxy.SocketEventArgs);

                    return;
                }
                else
                {
                    //Send was finished, next round of send can be started
                    m_SendReceiveResetEvent.Set();
                }        
            }
            else
            {
                Close();
            }
        }

        public override void Close()
        {
            if (Client != null && Client.Connected)
            {
                SocketAsyncProxy.Reset();
                base.Close();
            }
        }
    }
}
