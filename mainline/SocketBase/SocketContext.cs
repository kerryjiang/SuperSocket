using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The base class of socketContext
    /// </summary>
    public class SocketContext
    {
        public SocketContext()
        {
            Charset = Encoding.Default;
            Status = SocketContextStatus.Healthy;
        }

        /// <summary>
        /// Gets or sets the charset.
        /// </summary>
        /// <value>The charset.</value>
        public Encoding Charset { get; set; }

        /// <summary>
        /// Gets or sets the previous command.
        /// </summary>
        /// <value>
        /// The prev command.
        /// </value>
        public string PrevCommand { get; set; }

        /// <summary>
        /// Gets or sets the current executing command.
        /// </summary>
        /// <value>
        /// The current command.
        /// </value>
        public string CurrentCommand { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public SocketContextStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>
        /// The data context.
        /// </value>
        public object DataContext { get; set; }
    }
}
