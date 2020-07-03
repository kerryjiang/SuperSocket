using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket.Server
{
    public abstract class HostBuilderAdapter<THostBuilder> : IHostBuilder
        where THostBuilder : HostBuilderAdapter<THostBuilder>
    {
        protected IHostBuilder HostBuilder { get; private set; }

        public HostBuilderAdapter()
            : this(args: null)
        {
            
        }

        public HostBuilderAdapter(string[] args)
            : this(Host.CreateDefaultBuilder(args))
        {
            
        }

        public HostBuilderAdapter(IHostBuilder hostBuilder)
        {
            HostBuilder = hostBuilder;
        }

        public IDictionary<object, object> Properties => HostBuilder.Properties;

        public virtual IHost Build()
        {
            return HostBuilder.Build();
        }

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return ConfigureAppConfiguration(configureDelegate);
        }

        public virtual THostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            HostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            return ConfigureContainer<TContainerBuilder>(configureDelegate);
        }

        public virtual THostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            HostBuilder.ConfigureContainer<TContainerBuilder>(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            return ConfigureHostConfiguration(configureDelegate);
        }

        public THostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            HostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        public virtual THostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            HostBuilder.ConfigureServices(configureDelegate);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            return UseServiceProviderFactory<TContainerBuilder>(factory);
        }

        public virtual THostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            HostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this as THostBuilder;
        }

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            return UseServiceProviderFactory<TContainerBuilder>(factory);
        }

        public virtual THostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            HostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this as THostBuilder;
        }
    }
}