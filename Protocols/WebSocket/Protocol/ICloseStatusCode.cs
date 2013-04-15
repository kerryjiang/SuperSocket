using System;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// Close status code interface
    /// </summary>
    public interface ICloseStatusCode
    {
        /// <summary>
        /// Gets the code for extension not match.
        /// </summary>
        int ExtensionNotMatch { get; }

        /// <summary>
        /// Gets the code for going away.
        /// </summary>
        int GoingAway { get; }

        /// <summary>
        /// Gets the code for invalid UT f8.
        /// </summary>
        int InvalidUTF8 { get; }

        /// <summary>
        /// Gets the code for normal closure.
        /// </summary>
        int NormalClosure { get; }

        /// <summary>
        /// Gets the code for not acceptable data.
        /// </summary>
        int NotAcceptableData { get; }

        /// <summary>
        /// Gets the code for protocol error.
        /// </summary>
        int ProtocolError { get; }

        /// <summary>
        /// Gets the code for TLS handshake failure.
        /// </summary>
        int TLSHandshakeFailure { get; }

        /// <summary>
        /// Gets the code for too large frame.
        /// </summary>
        int TooLargeFrame { get; }

        /// <summary>
        /// Gets the code for unexpected condition.
        /// </summary>
        int UnexpectedCondition { get; }

        /// <summary>
        /// Gets the code for violate policy.
        /// </summary>
        int ViolatePolicy { get; }

        /// <summary>
        /// Gets the code for no status code.
        /// </summary>
        int NoStatusCode { get; }
    }
}
