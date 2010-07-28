using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Resources;
using System.Reflection;
using System.IO;

namespace SuperSocket.Test.Command
{
    public class RECEL : CommandBase<TestSession>
    {
        protected override void Execute(TestSession session, CommandInfo commandData)
        {
            int length = int.Parse(commandData[0]);

            MemoryStream ms = new MemoryStream();
            session.SocketSession.ReceiveData(ms, length);
            byte[] data = ms.ToArray();
            session.SocketSession.SendResponse(session.AppContext, data);
        }
    }
}
