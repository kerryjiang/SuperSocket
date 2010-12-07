using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test.Command
{
    public class NUM : StringCommandBase<TestSession>
    {
        public const string ReplyFormat = "325 received {0}!";

        public override void ExecuteCommand(TestSession session, StringCommandInfo commandData)
        {
            session.SendResponse(string.Format(ReplyFormat, commandData.Data));
        }

        public override string Name
        {
            get
            {
                return "325";
            }
        }
    }
}
