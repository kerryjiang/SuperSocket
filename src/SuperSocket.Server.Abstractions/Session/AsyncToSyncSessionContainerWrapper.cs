using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Wraps an asynchronous session container to provide synchronous operations.
    /// </summary>
    public class AsyncToSyncSessionContainerWrapper : ISessionContainer
    {
        IAsyncSessionContainer _asyncSessionContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncToSyncSessionContainerWrapper"/> class.
        /// </summary>
        /// <param name="asyncSessionContainer">The asynchronous session container to wrap.</param>
        public AsyncToSyncSessionContainerWrapper(IAsyncSessionContainer asyncSessionContainer)
        {
            _asyncSessionContainer = asyncSessionContainer;
        }

        /// <summary>
        /// Gets a session by its ID synchronously.
        /// </summary>
        /// <param name="sessionID">The ID of the session.</param>
        /// <returns>The session with the specified ID.</returns>
        public IAppSession GetSessionByID(string sessionID)
        {
            return _asyncSessionContainer.GetSessionByIDAsync(sessionID).Result;
        }

        /// <summary>
        /// Gets the count of active sessions synchronously.
        /// </summary>
        /// <returns>The count of active sessions.</returns>
        public int GetSessionCount()
        {
            return _asyncSessionContainer.GetSessionCountAsync().Result;
        }

        /// <summary>
        /// Gets sessions that match the specified criteria synchronously.
        /// </summary>
        /// <param name="criteria">The criteria to filter sessions.</param>
        /// <returns>The matching sessions.</returns>
        public IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria)
        {
            return _asyncSessionContainer.GetSessionsAsync(criteria).Result;
        }

        /// <summary>
        /// Gets sessions of a specific type that match the specified criteria synchronously.
        /// </summary>
        /// <typeparam name="TAppSession">The type of the session.</typeparam>
        /// <param name="criteria">The criteria to filter sessions.</param>
        /// <returns>The matching sessions.</returns>
        public IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria) where TAppSession : IAppSession
        {
            return _asyncSessionContainer.GetSessionsAsync<TAppSession>(criteria).Result;
        }
    }
}