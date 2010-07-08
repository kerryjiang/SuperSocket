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
        AutoResetEvent m_SendResetEvent = new AutoResetEvent(true);

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

            bool willRaiseEvent = Client.ReceiveAsync(this.SocketAsyncProxy.ReceiveEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(this.SocketAsyncProxy.ReceiveEventArgs);
            }
        }

        public override void SendResponse(SocketContext context, string message)
        {
            m_SendResetEvent.WaitOne();

            if (string.IsNullOrEmpty(message))
                return;

            if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

            AsyncUserToken token = this.SocketAsyncProxy.SendEventArgs.UserToken as AsyncUserToken;
            token.SendBuffer = context.Charset.GetBytes(message);
            token.Offset = 0;
            if (this.SocketAsyncProxy.SendEventArgs.Buffer.Length >= token.SendBuffer.Length)
            {
                Buffer.BlockCopy(token.SendBuffer, 0, this.SocketAsyncProxy.SendEventArgs.Buffer, 0, token.SendBuffer.Length);
                this.SocketAsyncProxy.SendEventArgs.SetBuffer(0, token.SendBuffer.Length);
                token.SendBuffer = new byte[0];
            }
            else
            {
                Buffer.BlockCopy(token.SendBuffer, 0, this.SocketAsyncProxy.SendEventArgs.Buffer, 0, this.SocketAsyncProxy.SendEventArgs.Buffer.Length);
                this.SocketAsyncProxy.SendEventArgs.SetBuffer(0, this.SocketAsyncProxy.SendEventArgs.Buffer.Length);
                token.Offset = token.Offset + this.SocketAsyncProxy.SendEventArgs.Buffer.Length;
            }

            if (!Client.SendAsync(this.SocketAsyncProxy.SendEventArgs))
                ProcessSend(this.SocketAsyncProxy.SendEventArgs);
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

                    if (this.SocketAsyncProxy.SendEventArgs.Buffer.Length >= leftBytes)
                    {
                        Buffer.BlockCopy(token.SendBuffer, token.Offset, this.SocketAsyncProxy.SendEventArgs.Buffer, 0, token.SendBuffer.Length);
                        this.SocketAsyncProxy.SendEventArgs.SetBuffer(0, token.SendBuffer.Length);
                        token.SendBuffer = new byte[0];
                    }
                    else
                    {
                        Buffer.BlockCopy(token.SendBuffer, token.Offset, this.SocketAsyncProxy.SendEventArgs.Buffer, 0, this.SocketAsyncProxy.SendEventArgs.Buffer.Length);
                        this.SocketAsyncProxy.SendEventArgs.SetBuffer(0, this.SocketAsyncProxy.SendEventArgs.Buffer.Length);
                        token.Offset = token.Offset + this.SocketAsyncProxy.SendEventArgs.Buffer.Length;
                    }

                    if (!Client.SendAsync(this.SocketAsyncProxy.SendEventArgs))
                        ProcessSend(this.SocketAsyncProxy.SendEventArgs);

                    return;
                }
                else
                {
                    //Send was finished, next round of send can be started
                    m_SendResetEvent.Set();
                }        
            }
            else
            {
                Close();
            }
        }

        public override void Close()
        {
            SocketAsyncProxy.Reset();
            base.Close();
        }
    }
}
