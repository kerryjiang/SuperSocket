using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.CommandFilter
{
    [LogTimeCommandFilter]
    public class LOGIN : StringCommandBase<MyAppSession>
    {
        public override void ExecuteCommand(MyAppSession session, StringPackageInfo requestInfo)
        {
            if (requestInfo.Parameters == null || requestInfo.Parameters.Length != 2)
                return;

            var username = requestInfo.Parameters[0];
            var password = requestInfo.Parameters[1];

            if("kerry".Equals(username) && "123456".Equals(password))
            {
                session.IsLoggedIn = true;
            }
        }
    }
}
