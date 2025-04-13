using System;
using Microsoft.Extensions.Hosting;

namespace SuperSocket.Server
{
    /// <summary>
    /// Defines a method to configure a container builder in the context of a host.
    /// </summary>
    internal interface IConfigureContainerAdapter
    {
        /// <summary>
        /// Configures the container builder with the specified host context.
        /// </summary>
        /// <param name="hostContext">The context of the host.</param>
        /// <param name="containerBuilder">The container builder to configure.</param>
        void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IConfigureContainerAdapter"/> for a specific container builder type.
    /// </summary>
    /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
    internal class ConfigureContainerAdapter<TContainerBuilder> : IConfigureContainerAdapter
    {
        private Action<HostBuilderContext, TContainerBuilder> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureContainerAdapter{TContainerBuilder}"/> class with the specified action.
        /// </summary>
        /// <param name="action">The action to configure the container builder.</param>
        /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
        public ConfigureContainerAdapter(Action<HostBuilderContext, TContainerBuilder> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Configures the container builder with the specified host context.
        /// </summary>
        /// <param name="hostContext">The context of the host.</param>
        /// <param name="containerBuilder">The container builder to configure.</param>
        public void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder)
        {
            _action(hostContext, (TContainerBuilder)containerBuilder);
        }
    }
}