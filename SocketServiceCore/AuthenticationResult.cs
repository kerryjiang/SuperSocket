using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Authentication result
    /// </summary>
    public enum AuthenticationResult
    {
        /// <summary>
        /// The user does not exist
        /// </summary>
        NotExist,
        /// <summary>
        /// The password is wrong
        /// </summary>
        PasswordError,
        /// <summary>
        /// The password is correct
        /// </summary>
        Success
    }
}
