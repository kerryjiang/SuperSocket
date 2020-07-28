using Super.Engine;
using SuperSocket.Command;
using System;
using System.Threading.Tasks;

namespace Super.Command
{
    [Command(Key = (short)0x0002)]
    public class Heartbeat : OnlineCommandBase
    {
        public override ValueTask ExecuteAsync(IOnlineSession session, OnlinePackageInfo package)
        {
            Console.WriteLine("Receive heartbeat");
            return new ValueTask(Task.CompletedTask);
        }
    }
}
