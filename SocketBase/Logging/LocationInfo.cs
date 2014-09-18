using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase.Logging
{
    /// <summary>
    /// location info
    /// </summary>
    public struct LocationInfo
    {
        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        /// <value>
        /// The name of the class.
        /// </value>
        public string ClassName { get; set; }
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public string LineNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public string MethodName { get; set; }
        /// <summary>
        /// Gets or sets the full information.
        /// </summary>
        /// <value>
        /// The full information.
        /// </value>
        public string FullInfo { get; set; }
        /// <summary>
        /// Gets or sets the stack frames.
        /// </summary>
        /// <value>
        /// The stack frames.
        /// </value>
        public StackFrame[] StackFrames { get; set; }
    }
}
