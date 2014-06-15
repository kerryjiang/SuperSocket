using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test.Command
{
    public class ECHO : StringCommandBase<TestSession>
    {
        private static int m_Sent;
        public static string LastSent { get; private set; }

        public override void ExecuteCommand(TestSession session, StringPackageInfo requestInfo)
        {
            //Console.WriteLine("R:" + requestInfo.Body);
            LastSent = requestInfo.Body;
            session.Send(requestInfo.Body);
            Interlocked.Increment(ref m_Sent);
        }

        public static int Sent
        {
            get { return m_Sent; }
        }

        public static void Reset()
        {
            m_Sent = 0;
        }
    }
}
