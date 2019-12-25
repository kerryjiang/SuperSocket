using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Channel
{
    public static class SocketExtensions
    {
        internal static bool IsIgnorableSocketException(this SocketException se)
        {
            if (se.ErrorCode == 89)
                return true;

            if (se.ErrorCode == 125)
                return true;

            if (se.ErrorCode == 104)
                return true;

            if (se.ErrorCode == 54)
                return true;

            return false;
        }
    }
}
