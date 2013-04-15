using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.WebSocket.Protocol;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// Plain text fragment
    /// </summary>
    class PlainFragment : IWebSocketFragment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlainFragment"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PlainFragment(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public string Key
        {
            get { return OpCode.PlainTag; }
        }
    }
}
