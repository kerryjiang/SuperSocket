using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    /// <summary>
    /// A generic session factory for creating instances of a specific session type.
    /// </summary>
    /// <typeparam name="TSession">The type of session to create.</typeparam>
    public class GenericSessionFactory<TSession> : ISessionFactory
        where TSession : IAppSession
    {
        /// <summary>
        /// Gets the type of session created by this factory.
        /// </summary>
        public Type SessionType => typeof(TSession);

        /// <summary>
        /// Gets the service provider used to create session instances.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSessionFactory{TSession}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to create session instances.</param>
        public GenericSessionFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new session instance.
        /// </summary>
        /// <returns>A new instance of the session.</returns>
        public IAppSession Create()
        {
            return ActivatorUtilities.CreateInstance<TSession>(ServiceProvider);
        }
    }
}