using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Session register interface
    /// </summary>
    public interface ISessionRegister
    {
        /// <summary>
        /// Tries the register session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool RegisterSession(IAppSession session);
    }

    /// <summary>
    /// The basic interface for the session container
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    public interface ISessionContainer<TAppSession> : IEnumerable<TAppSession>
        where TAppSession : IAppSession
    {

        /// <summary>
        /// Initializes the session container with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        bool Initialize(IServerConfig config);

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the session by ID.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns></returns>
        TAppSession GetSessionByID(string sessionID);


        /// <summary>
        /// Tries the register session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool TryRegisterSession(TAppSession session);

        /// <summary>
        /// Tries to unregisters the session.
        /// </summary>
        /// <param name="sessionID">The session ID.</param>
        /// <returns>true, unregister sucessfully; false, the session doesn't exist</returns>
        bool TryUnregisterSession(string sessionID);
    }
}
