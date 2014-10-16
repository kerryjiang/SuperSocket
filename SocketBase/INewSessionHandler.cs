using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The new session handler interface
    /// </summary>
    public interface INewSessionHandler
    {
        /// <summary>
        /// Initializes the specified session register.
        /// </summary>
        /// <param name="sessionRegister">The session register.</param>
        void Initialize(ISessionRegister sessionRegister);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Accepts the new session.
        /// </summary>
        /// <param name="session">The session.</param>
        void AcceptNewSession(IAppSession session);
    }
}
