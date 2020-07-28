using Super.Engine;
using SuperSocket.Command;
using System;
using System.Buffers;
using System.Threading.Tasks;

namespace Super.Command
{
    [Command(Key = (short)0x0001)]
    public class Register : OnlineCommandBase<ReadOnlySequence<byte>>
    {
        public override ValueTask ExecuteAsync(IOnlineSession session, OnlinePackageInfo<ReadOnlySequence<byte>> package)
        {
            Console.WriteLine("Receive message");
            return new ValueTask(Task.CompletedTask);
        }
    }
}
