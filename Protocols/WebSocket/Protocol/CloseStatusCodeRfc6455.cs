using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// Close status code for rfc6455
    /// </summary>
    public class CloseStatusCodeRfc6455 : ICloseStatusCode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloseStatusCodeRfc6455"/> class.
        /// </summary>
        public CloseStatusCodeRfc6455()
        {
            NormalClosure = 1000;
            GoingAway = 1001;
            ProtocolError = 1002;
            NotAcceptableData = 1003;
            TooLargeFrame = 1009;
            InvalidUTF8 = 1007;
            ViolatePolicy = 1008;
            ExtensionNotMatch = 1010;
            UnexpectedCondition = 1011;
            NoStatusCode = 1005;
        }

        /// <summary>
        /// Gets the code for normal closure.
        /// </summary>
        public int NormalClosure { get; private set; }

        /// <summary>
        /// Gets the code for going away.
        /// </summary>
        public int GoingAway { get; private set; }

        /// <summary>
        /// Gets the code for protocol error.
        /// </summary>
        public int ProtocolError { get; private set; }

        /// <summary>
        /// Gets the code for not acceptable data.
        /// </summary>
        public int NotAcceptableData { get; private set; }

        /// <summary>
        /// Gets the code for too large frame.
        /// </summary>
        public int TooLargeFrame { get; private set; }

        /// <summary>
        /// Gets the code for invalid UT f8.
        /// </summary>
        public int InvalidUTF8 { get; private set; }

        /// <summary>
        /// Gets the code for violate policy.
        /// </summary>
        public int ViolatePolicy { get; private set; }

        /// <summary>
        /// Gets the code for extension not match.
        /// </summary>
        public int ExtensionNotMatch { get; private set; }

        /// <summary>
        /// Gets the code for unexpected condition.
        /// </summary>
        public int UnexpectedCondition { get; private set; }

        /// <summary>
        /// Gets the code for TLS handshake failure.
        /// </summary>
        public int TLSHandshakeFailure { get; private set; }

        /// <summary>
        /// Gets the code for no status code.
        /// </summary>
        public int NoStatusCode { get; private set; }
    }
}
