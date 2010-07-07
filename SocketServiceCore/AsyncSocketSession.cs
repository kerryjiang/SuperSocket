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
		protected override void Start(SocketContext context)
		{
			Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            SocketAsyncProxy.SocketSession = this;
            ((AsyncUserToken)SocketAsyncProxy.EventArgs.UserToken).SocketContext = context;
			SayWelcome();
		}

        public override void SendResponse(SocketContext context, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

            AsyncUserToken token = this.SocketAsyncProxy.EventArgs.UserToken as AsyncUserToken;
            token.SendBuffer = context.Charset.GetBytes(message);
            token.Offset = 0;
            if (this.SocketAsyncProxy.EventArgs.Buffer.Length >= token.SendBuffer.Length)
            {
                Buffer.BlockCopy(token.SendBuffer, 0, this.SocketAsyncProxy.EventArgs.Buffer, 0, token.SendBuffer.Length);
                this.SocketAsyncProxy.EventArgs.SetBuffer(0, token.SendBuffer.Length);
                this.SocketAsyncProxy.EventArgs.SendPacketsSendSize = token.SendBuffer.Length;
                token.SendBuffer = new byte[0];
            }
            else
            {
                Buffer.BlockCopy(token.SendBuffer, 0, this.SocketAsyncProxy.EventArgs.Buffer, 0, this.SocketAsyncProxy.EventArgs.Buffer.Length);
                this.SocketAsyncProxy.EventArgs.SetBuffer(0, this.SocketAsyncProxy.EventArgs.Buffer.Length);
                token.Offset = token.Offset + this.SocketAsyncProxy.EventArgs.Buffer.Length;
            }

            if (!Client.SendAsync(this.SocketAsyncProxy.EventArgs))
                ProcessSend(this.SocketAsyncProxy.EventArgs);
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                string commandLine = Encoding.ASCII.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                CommandInfo cmdInfo = new CommandInfo(commandLine.Trim());
                ExecuteCommand(cmdInfo);
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

                    if (this.SocketAsyncProxy.EventArgs.Buffer.Length >= leftBytes)
                    {
                        Buffer.BlockCopy(token.SendBuffer, token.Offset, this.SocketAsyncProxy.EventArgs.Buffer, 0, token.SendBuffer.Length);
                        this.SocketAsyncProxy.EventArgs.SetBuffer(0, token.SendBuffer.Length);
                        token.SendBuffer = new byte[0];
                    }
                    else
                    {
                        Buffer.BlockCopy(token.SendBuffer, token.Offset, this.SocketAsyncProxy.EventArgs.Buffer, 0, this.SocketAsyncProxy.EventArgs.Buffer.Length);
                        this.SocketAsyncProxy.EventArgs.SetBuffer(0, this.SocketAsyncProxy.EventArgs.Buffer.Length);
                        token.Offset = token.Offset + this.SocketAsyncProxy.EventArgs.Buffer.Length;
                    }

                    if (!Client.SendAsync(this.SocketAsyncProxy.EventArgs))
                        ProcessSend(this.SocketAsyncProxy.EventArgs);

                    return;
                }

                //this.SocketAsyncProxy.EventArgs.SetBuffer(0, this.SocketAsyncProxy.EventArgs.Buffer.Length);
                if (token.SocketContext.RequireRead)
                {
                    // read the next block of data send from the client
                    bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);
                    }
                }
            }
            else
            {
                Close();
            }
        }

        public override void Close()
        {
            SocketAsyncProxy.SocketSession = null;
            base.Close();
        }
    }
}
