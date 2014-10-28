using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperWebSocketTest.Command
{
    public class ECHO : StringCommandBase
    {
        public override void ExecuteCommand(AppSession session, StringPackageInfo requestInfo)
        {
            var paramsArray = requestInfo.Body.Split(' ');
            for (var i = 0; i < paramsArray.Length; i++)
            {
                session.Send(paramsArray[i]);
            }
        }
    }
}
