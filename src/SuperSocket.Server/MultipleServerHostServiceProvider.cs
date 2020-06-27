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
    class MultipleServerHostServiceProvider : IServiceProvider
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IServiceProvider HostServiceProvider { get; private set; }

        public MultipleServerHostServiceProvider(IServiceProvider serviceProvider, IServiceProvider hostServiceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (hostServiceProvider == null)
                throw new ArgumentNullException(nameof(hostServiceProvider));

            ServiceProvider = serviceProvider;
            HostServiceProvider = hostServiceProvider;
        }

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType) ?? HostServiceProvider.GetService(serviceType);
        }
    }
}