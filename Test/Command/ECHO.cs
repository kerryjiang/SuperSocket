using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class ECHO : StringCommandBase<TestSession>
    {
        private static int m_Sent;

        public override void ExecuteCommand(TestSession session, StringRequestInfo requestInfo)
        {
            //Console.WriteLine("R:" + requestInfo.Body);
            session.Send(requestInfo.Body);
            Interlocked.Increment(ref m_Sent);
        }

        public static int Sent
        {
            get { return m_Sent; }
        }
    }
}
