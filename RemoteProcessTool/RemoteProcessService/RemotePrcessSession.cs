using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace RemoteProcessService
{
    public class RemotePrcessSession : AppSession<RemotePrcessSession>
    {
        private SocketContext m_Context;

        protected override void OnClosed()
        {
            
        }

        protected override void OnInit()
        {
            m_Context = new SocketContext();
        }

        public override void SayWelcome()
        {
            SendResponse("Welcome to use this tool!");
        }

        public override void HandleExceptionalError(Exception e)
        {
            SendResponse("An error has occurred in server side! Error message: " + e.Message + "!");
        }

        public override SocketContext Context
        {
            get { return m_Context; }
        }

        public RemoteProcessServer CurrentServer
        {
            get { return this.AppServer as RemoteProcessServer; }
        }
    }
}
