using System;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Represents a factory for creating application sessions.
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        /// Creates a new instance of an application session.
        /// </summary>
        /// <returns>A newly created application session.</returns>
        IAppSession Create();

        /// <summary>
        /// Gets the type of session this factory creates.
        /// </summary>
        Type SessionType { get; }
    }
}