using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class RECEM : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringRequestInfo commandData)
        {
            //byte[] mark = Encoding.ASCII.GetBytes(string.Format("{0}.{0}", Environment.NewLine));
            //MemoryStream ms = new MemoryStream();
            //session.SocketSession.ReceiveData(ms, mark);
            //byte[] data = ms.ToArray();
            //session.SocketSession.SendResponse(data);
        }
    }
}
