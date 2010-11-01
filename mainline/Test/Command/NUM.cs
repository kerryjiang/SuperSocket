using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.Test.Command
{
    public class NUM : CommandBase<TestSession>
    {
        public const string ReplyFormat = "325 received {0}!";

        protected override void Execute(TestSession session, CommandInfo commandData)
        {
            session.SendResponse(string.Format(ReplyFormat, commandData.Param));
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
