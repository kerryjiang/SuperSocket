using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SuperSocket.Server.Host
{
    public class SuperSocketWebApplicationBuilder : IHostApplicationBuilder
    {
        private readonly IHostApplicationBuilder _hostApplicationBuilder;

        public IDictionary<object, object> Properties => _hostApplicationBuilder.Properties;

        public IConfigurationManager Configuration => _hostApplicationBuilder.Configuration;

        public IHostEnvironment Environment => _hostApplicationBuilder.Environment;

        public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;

        public IMetricsBuilder Metrics => _hostApplicationBuilder.Metrics;

        public IServiceCollection Services => _hostApplicationBuilder.Services;

        internal SuperSocketWebApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder)
        {
            _hostApplicationBuilder = hostApplicationBuilder;
        }

        public IHostBuilder Host
        {
            get
            {
                return _hostApplicationBuilder.GetType()
                    .GetProperty("Host", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .GetValue(_hostApplicationBuilder) as IHostBuilder;
            }
        }

        public IHost Build()
        {
            var host = _hostApplicationBuilder
                .GetType()
                .GetMethod(nameof(Build))
                .Invoke(_hostApplicationBuilder, Array.Empty<object>()) as IHost;

            host.Services.GetService<MultipleServerHostBuilder>()?.AdaptMultipleServerHost(host.Services);

            return host;
        }

        void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder> configure)
        {
            _hostApplicationBuilder.ConfigureContainer<TContainerBuilder>(factory, configure);
        }
    }
}