using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Represents an exception that occurs during protocol handling.
    /// </summary>
    public class ProtocolException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolException"/> class with the specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="exception">The exception that caused the current exception.</param>
        public ProtocolException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ProtocolException(string message)
            : base(message)
        {
        }
    }
}