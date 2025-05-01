using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Server.Host
{
    /// <summary>
    /// Provides a builder for configuring and building a SuperSocket web application.
    /// </summary>
    public class SuperSocketApplicationBuilder : IHostApplicationBuilder
    {
        private readonly IHostApplicationBuilder _hostApplicationBuilder;

        /// <summary>
        /// Gets the properties associated with the application builder.
        /// </summary>
        public IDictionary<object, object> Properties => _hostApplicationBuilder.Properties;

        /// <summary>
        /// Gets the configuration manager for the application.
        /// </summary>
        public IConfigurationManager Configuration => _hostApplicationBuilder.Configuration;

        /// <summary>
        /// Gets the hosting environment for the application.
        /// </summary>
        public IHostEnvironment Environment => _hostApplicationBuilder.Environment;

        /// <summary>
        /// Gets the logging builder for configuring logging services.
        /// </summary>
        public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;

        /// <summary>
        /// Gets the metrics builder for configuring metrics services.
        /// </summary>
        public IMetricsBuilder Metrics => _hostApplicationBuilder.Metrics;

        /// <summary>
        /// Gets the service collection for the application.
        /// </summary>
        public IServiceCollection Services => _hostApplicationBuilder.Services;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperSocketApplicationBuilder"/> class.
        /// </summary>
        /// <param name="hostApplicationBuilder">The underlying host application builder.</param>
        internal SuperSocketApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder)
        {
            _hostApplicationBuilder = hostApplicationBuilder;
        }

        /// <summary>
        /// Gets the host builder associated with the application.
        /// </summary>
        public IHostBuilder Host
        {
            get
            {
                var hostProperty = _hostApplicationBuilder.GetType()
                    .GetProperty("Host", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (hostProperty != null)
                {
                    return hostProperty.GetValue(_hostApplicationBuilder) as IHostBuilder;
                }

                var asHostBuilderMethod = _hostApplicationBuilder.GetType().GetMethod("AsHostBuilder", BindingFlags.NonPublic | BindingFlags.Instance);

                if (asHostBuilderMethod != null)
                {
                    return asHostBuilderMethod.Invoke(_hostApplicationBuilder, Array.Empty<object>()) as IHostBuilder;
                }

                throw new InvalidOperationException("Unable to retrieve the host builder from the application builder.");
            }
        }

        /// <summary>
        /// Builds the host for the application.
        /// </summary>
        /// <returns>The built host.</returns>
        public IHost Build()
        {
            var host = _hostApplicationBuilder
                .GetType()
                .GetMethod(nameof(Build))
                .Invoke(_hostApplicationBuilder, Array.Empty<object>()) as IHost;

            host.Services.GetService<MultipleServerHostBuilder>()?.AdaptMultipleServerHost(host.Services);

            return host;
        }

        /// <summary>
        /// Configures the container for the application.
        /// </summary>
        /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
        /// <param name="factory">The service provider factory to use.</param>
        /// <param name="configure">The action to configure the container.</param>
        void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder> configure)
        {
            _hostApplicationBuilder.ConfigureContainer<TContainerBuilder>(factory, configure);
        }
    }
}