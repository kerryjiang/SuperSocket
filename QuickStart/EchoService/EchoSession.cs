using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace EchoService
{
    public class EchoSession : AppSession<EchoSession>
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
            SendResponse("Welcome to EchoServer!");
        }

        public override void HandleExceptionalError(Exception e)
        {
            SendResponse("Server side error occurred!");
        }

        public override SocketContext Context
        {
            get { return m_Context; }
        }
    }
}
