using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class RECEL : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringRequestInfo commandData)
        {
            //int length = int.Parse(commandData[0]);

            //MemoryStream ms = new MemoryStream();
            //session.SocketSession.ReceiveData(ms, length);
            //byte[] data = ms.ToArray();
            //session.SocketSession.SendResponse(data);
        }
    }
}
