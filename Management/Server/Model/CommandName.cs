using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Server.Model
{
    /// <summary>
    /// Predefined command names
    /// </summary>
    public class CommandName
    {
        /// <summary>
        /// Server infromation update
        /// </summary>
        public const string UPDATE = "UPDATE";

        /// <summary>
        /// Login
        /// </summary>
        public const string LOGIN = "LOGIN";

        /// <summary>
        /// Start server instance
        /// </summary>
        public const string START = "START";

        /// <summary>
        /// Stop server instance
        /// </summary>
        public const string STOP = "STOP";
    }
}
