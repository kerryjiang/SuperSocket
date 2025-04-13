using System;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// Default implementation of the session factory.
    /// </summary>
    class DefaultSessionFactory : ISessionFactory
    {
        /// <summary>
        /// Gets the type of the session created by this factory.
        /// </summary>
        public Type SessionType => typeof(AppSession);

        /// <summary>
        /// Creates a new instance of <see cref="IAppSession"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="AppSession"/>.</returns>
        public IAppSession Create()
        {
            return new AppSession();
        }
    }
}