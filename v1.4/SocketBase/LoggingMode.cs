using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    public enum LoggingMode
    {
        /// <summary>
        /// All server instances share same logging files
        /// </summary>
        ShareFile,

        /// <summary>
        /// All server instances have independant logging files
        /// </summary>
        IndependantFile,

        /// <summary>
        /// Using console logging, only works in console application
        /// </summary>
        Console
    }
}
