using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Provides extension methods for working with sockets.
    /// </summary>
    public static class SocketExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="SocketException"/> is ignorable.
        /// </summary>
        /// <param name="se">The <see cref="SocketException"/> to evaluate.</param>
        /// <returns><c>true</c> if the exception is ignorable; otherwise, <c>false</c>.</returns>
        public static bool IsIgnorableSocketException(this SocketException se)
        {
            switch (se.SocketErrorCode)
            {
                case (SocketError.OperationAborted):
                case (SocketError.ConnectionReset):
                case (SocketError.ConnectionAborted):
                case (SocketError.TimedOut):
                case (SocketError.NetworkReset):
                    return true;
                default:
                    return false;
            }
        }
    }
}
