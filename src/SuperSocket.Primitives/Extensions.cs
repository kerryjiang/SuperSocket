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

        public static IPEndPoint GetListenEndPoint(this ListenOptions listenOptions)
        {
            var ip = listenOptions.Ip;
            var port = listenOptions.Port;

            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.Any;
            }
            else if ("IpV6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = IPAddress.IPv6Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }

            return new IPEndPoint(ipAddress, port);
        }
    }
}