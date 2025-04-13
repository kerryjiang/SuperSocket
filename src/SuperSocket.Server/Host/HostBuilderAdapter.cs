using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GenericHost = Microsoft.Extensions.Hosting.Host;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Provides an adapter for customizing and extending the behavior of a host builder.
    /// </summary>
    /// <typeparam name="THostBuilder">The type of the host builder being adapted.</typeparam>
    public abstract class HostBuilderAdapter<THostBuilder> : IHostBuilder
        where THostBuilder : HostBuilderAdapter<THostBuilder>
    {
        /// <summary>
        /// Gets the underlying host builder being adapted.
        /// </summary>
        protected IHostBuilder HostBuilder { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilderAdapter{THostBuilder}"/> class with default settings.
        /// </summary>
        public HostBuilderAdapter()
            : this(args: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilderAdapter{THostBuilder}"/> class with the specified arguments.
        /// </summary>
        /// <param name="args">The command-line arguments for the host builder.</param>
        public HostBuilderAdapter(string[] args)
            : this(GenericHost.CreateDefaultBuilder(args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilderAdapter{THostBuilder}"/> class with the specified host builder.
        /// </summary>
        /// <param name="hostBuilder">The host builder to adapt.</param>
        public HostBuilderAdapter(IHostBuilder hostBuilder)
        {
            HostBuilder = hostBuilder;
        }

        /// <summary>
        /// Gets the properties associated with the host builder.
        /// </summary>
        public IDictionary<object, object> Properties => HostBuilder.Properties;

        /// <summary>
        /// Builds the host.
        /// </summary>
        /// <returns>The built host.</returns>
        public virtual IHost Build()
        {
            return HostBuilder.Build();
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return ConfigureAppConfiguration(configureDelegate);
        }

        /// <summary>
        /// Configures the application configuration for the host.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure the application configuration.</param>
        /// <returns>The adapted host builder.</returns>
        public virtual THostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            HostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            return ConfigureContainer<TContainerBuilder>(configureDelegate);
        }

        /// <summary>
        /// Configures the container for the host.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="configureDelegate">The delegate to configure the container.</param>
        /// <returns>The adapted host builder.</returns>
        public virtual THostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            HostBuilder.ConfigureContainer(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            return ConfigureHostConfiguration(configureDelegate);
        }

        /// <summary>
        /// Configures the host configuration.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure the host configuration.</param>
        /// <returns>The adapted host builder.</returns>
        public THostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            HostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        /// <summary>
        /// Configures the services for the host.
        /// </summary>
        /// <param name="configureDelegate">The delegate to configure the services.</param>
        /// <returns>The adapted host builder.</returns>
        public virtual THostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            HostBuilder.ConfigureServices(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            return UseServiceProviderFactory<TContainerBuilder>(factory);
        }

        /// <summary>
        /// Uses the specified service provider factory for the host.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="factory">The service provider factory to use.</param>
        /// <returns>The adapted host builder.</returns>
        public virtual THostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            HostBuilder.UseServiceProviderFactory(factory);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            return UseServiceProviderFactory<TContainerBuilder>(factory);
        }

        /// <summary>
        /// Uses the specified service provider factory for the host.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="factory">A function to create the service provider factory.</param>
        /// <returns>The adapted host builder.</returns>
        public virtual THostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            HostBuilder.UseServiceProviderFactory(factory);
            return this as THostBuilder;
        }
    }
}