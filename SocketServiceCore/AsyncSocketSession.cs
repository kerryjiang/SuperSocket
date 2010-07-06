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
			SayWelcome();            
            //if (!Client.ReceiveAsync(SocketAsyncProxy.EventArgs))
            //    ProcessReceive(SocketAsyncProxy.EventArgs);
		}

        public override void SendResponse(SocketContext context, string message)
        {
            byte[] data = context.Charset.GetBytes(message);
            this.SocketAsyncProxy.EventArgs.SetBuffer(data, 0, data.Length);
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
                CommandInfo cmdInfo = new CommandInfo(commandLine);
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
                // read the next block of data send from the client
                bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
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
