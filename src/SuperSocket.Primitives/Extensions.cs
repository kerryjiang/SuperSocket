using System;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket
{
    public static class Extensions
    {
        public static void DoNotAwait(this Task task)
        {
            
        }

        public static void DoNotAwait(this ValueTask task)
        {

        }

#if NETSTANDARD2_1
        private static readonly ValueTask _completedTask = new ValueTask();

        public static ValueTask GetCompletedTask()
        {
            return _completedTask;
        }
#endif
    }
}