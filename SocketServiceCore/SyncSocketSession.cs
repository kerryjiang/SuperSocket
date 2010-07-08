using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.SocketServiceCore
{
    public class SyncSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
    {
        private Thread m_CommandThread = null;

        /// <summary>
        /// Starts the the session with specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Start(SocketContext context)
        {
            m_CommandThread = Thread.CurrentThread;

            //Client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            InitStream(context);

            SayWelcome();

            string commandLine;

            while (TryGetCommand(out commandLine))
            {
                LastActiveTime = DateTime.Now;
                context.Status = SocketContextStatus.Healthy;

                try
                {
                    ExecuteCommand(commandLine);                    

                    if (Client == null && !IsClosed)
                    {
                        //Has been closed
                        OnClose();
                        return;
                    }
                }
                catch (SocketException)
                {
                    Close();
                    break;
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e);
                    HandleExceptionalError(e);
                }
            }

            if (Client != null)
            {
                Close();
            }
            else if (!IsClosed)
            {
                OnClose();
            }
        }


        public override void Close()
        {
            base.Close();
        }
    }
}
