using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Authentication;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// It is the basic interface of SocketServer,
    /// SocketServer is the abstract server who really listen the comming sockets directly.
    /// </summary>
    public interface ISocketServer
    {
        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// Resets the session's security protocol.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="security">The security protocol.</param>
        void ResetSessionSecurity(IAppSession session, SslProtocols security);
        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the information of the sending queue pool.
        /// </summary>
        /// <value>
        /// The sending queue pool.
        /// </value>
        IPoolInfo SendingQueuePool { get; }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
