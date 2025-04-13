using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Wraps a synchronous session container to provide asynchronous operations.
    /// </summary>
    public class SyncToAsyncSessionContainerWrapper : IAsyncSessionContainer
    {
        ISessionContainer _syncSessionContainer;

        /// <summary>
        /// Gets the underlying synchronous session container.
        /// </summary>
        public ISessionContainer SessionContainer
        {
            get { return _syncSessionContainer; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncToAsyncSessionContainerWrapper"/> class.
        /// </summary>
        /// <param name="syncSessionContainer">The synchronous session container to wrap.</param>
        public SyncToAsyncSessionContainerWrapper(ISessionContainer syncSessionContainer)
        {
            _syncSessionContainer = syncSessionContainer;
        }

        /// <summary>
        /// Gets a session by its ID asynchronously.
        /// </summary>
        /// <param name="sessionID">The ID of the session.</param>
        /// <returns>A task that represents the asynchronous operation, containing the session.</returns>
        public ValueTask<IAppSession> GetSessionByIDAsync(string sessionID)
        {
            return new ValueTask<IAppSession>(_syncSessionContainer.GetSessionByID(sessionID));
        }

        /// <summary>
        /// Gets the count of active sessions asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the session count.</returns>
        public ValueTask<int> GetSessionCountAsync()
        {
            return new ValueTask<int>(_syncSessionContainer.GetSessionCount());
        }

        /// <summary>
        /// Gets sessions that match the specified criteria asynchronously.
        /// </summary>
        /// <param name="criteria">The criteria to filter sessions.</param>
        /// <returns>A task that represents the asynchronous operation, containing the matching sessions.</returns>
        public ValueTask<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> criteria = null)
        {
            return new ValueTask<IEnumerable<IAppSession>>(_syncSessionContainer.GetSessions(criteria));
        }

        /// <summary>
        /// Gets sessions of a specific type that match the specified criteria asynchronously.
        /// </summary>
        /// <typeparam name="TAppSession">The type of the session.</typeparam>
        /// <param name="criteria">The criteria to filter sessions.</param>
        /// <returns>A task that represents the asynchronous operation, containing the matching sessions.</returns>
        public ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> criteria = null) where TAppSession : IAppSession
        {
            return new ValueTask<IEnumerable<TAppSession>>(_syncSessionContainer.GetSessions<TAppSession>(criteria));
        }
    }
}