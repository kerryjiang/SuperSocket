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
                if (EndsWith(e.Buffer, e.Offset, e.BytesTransferred, token.SocketContext.NewLineData))
                {
                    string commandLine = string.Empty;

                    if (token.ReceiveBuffer != null)
                    {
                        token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(e.BytesTransferred));
                        commandLine = token.SocketContext.Charset.GetString(token.ReceiveBuffer.ToArray());
                        token.ReceiveBuffer.Clear();
                        token.ReceiveBuffer = null;
                    }
                    else
                    {
                        commandLine = token.SocketContext.Charset.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    }
                    //this round receive has been finished, the buffer can be used for sending
                    m_SendReceiveResetEvent.Set();

                    commandLine = commandLine.Trim();
                    ExecuteCommand(commandLine);
                    //read the next block of data send from the client
                    StartReceive();
                }
                else
                {
                    //Put this round receive data into receive buffer
                    if (token.ReceiveBuffer == null)
                        token.ReceiveBuffer = new List<byte>();

                    token.ReceiveBuffer.AddRange(e.Buffer.Skip(e.Offset).Take(e.BytesTransferred));

                    //Continue receive
                    m_SendReceiveResetEvent.Set();
                    StartReceive();
                    return;
                }                
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

        private static bool EndsWith(byte[] buffer, int offset, int length, byte[] endData)
        {
            if (length < endData.Length)
                return false;

            for (int i = 1; i <= endData.Length; i++)
            {
                if (endData[endData.Length - i] != buffer[offset + length - i])
                    return false;
            }

            return true;
        }
    }
}
