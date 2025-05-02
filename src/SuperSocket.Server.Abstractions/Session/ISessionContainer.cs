using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Represents a container for managing and accessing sessions.
    /// </summary>
    public interface ISessionContainer
    {
        /// <summary>
        /// Gets a session by its unique identifier.
        /// </summary>
        /// <param name="sessionID">The unique identifier of the session.</param>
        /// <returns>The session if found; otherwise, null.</returns>
        IAppSession GetSessionByID(string sessionID);

        /// <summary>
        /// Gets the total count of sessions in the container.
        /// </summary>
        /// <returns>The number of sessions.</returns>
        int GetSessionCount();

        /// <summary>
        /// Gets sessions that match the specified criteria.
        /// </summary>
        /// <param name="criteria">The predicate used to filter sessions, or null to get all sessions.</param>
        /// <returns>An enumerable collection of sessions that match the criteria.</returns>
        IEnumerable<IAppSession> GetSessions(Predicate<IAppSession> criteria = null);

        /// <summary>
        /// Gets sessions of a specific type that match the specified criteria.
        /// </summary>
        /// <typeparam name="TAppSession">The type of sessions to retrieve.</typeparam>
        /// <param name="criteria">The predicate used to filter sessions, or null to get all sessions of the specified type.</param>
        /// <returns>An enumerable collection of sessions that match the criteria.</returns>
        IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> criteria = null)
            where TAppSession : IAppSession;
    }

    /// <summary>
    /// Represents an asynchronous container for managing and accessing sessions.
    /// </summary>
    public interface IAsyncSessionContainer
    {
        /// <summary>
        /// Gets a session by its unique identifier asynchronously.
        /// </summary>
        /// <param name="sessionID">The unique identifier of the session.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the session if found; otherwise, null.</returns>
        ValueTask<IAppSession> GetSessionByIDAsync(string sessionID);

        /// <summary>
        /// Gets the total count of sessions in the container asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of sessions.</returns>
        ValueTask<int> GetSessionCountAsync();

        /// <summary>
        /// Gets sessions that match the specified criteria asynchronously.
        /// </summary>
        /// <param name="criteria">The predicate used to filter sessions, or null to get all sessions.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of sessions that match the criteria.</returns>
        ValueTask<IEnumerable<IAppSession>> GetSessionsAsync(Predicate<IAppSession> criteria = null);

        /// <summary>
        /// Gets sessions of a specific type that match the specified criteria asynchronously.
        /// </summary>
        /// <typeparam name="TAppSession">The type of sessions to retrieve.</typeparam>
        /// <param name="criteria">The predicate used to filter sessions, or null to get all sessions of the specified type.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of sessions that match the criteria.</returns>
        ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> criteria = null)
            where TAppSession : IAppSession;
    }
}