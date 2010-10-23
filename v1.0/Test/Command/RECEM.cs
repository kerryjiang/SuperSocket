using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.IO;

namespace SuperSocket.Test.Command
{
    public class RECEM : CommandBase<TestSession>
    {
        protected override void Execute(TestSession session, CommandInfo commandData)
        {
            byte[] mark = Encoding.ASCII.GetBytes(string.Format("{0}.{0}", Environment.NewLine));
            MemoryStream ms = new MemoryStream();
            session.SocketSession.ReceiveData(ms, mark);
            byte[] data = ms.ToArray();
            session.SocketSession.SendResponse(session.AppContext, data);
        }
    }
}
