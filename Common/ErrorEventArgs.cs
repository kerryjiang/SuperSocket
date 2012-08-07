using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    /// <summary>
    /// EventArgs for error and exception
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ErrorEventArgs(string message)
        {
            Exception = new Exception(message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
