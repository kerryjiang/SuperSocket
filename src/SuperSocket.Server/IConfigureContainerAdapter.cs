using System;
using Microsoft.Extensions.Hosting;

namespace SuperSocket.Server
{
    internal interface IConfigureContainerAdapter
    {
        void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
    }

    internal class ConfigureContainerAdapter<TContainerBuilder> : IConfigureContainerAdapter
    {
        private Action<HostBuilderContext, TContainerBuilder> _action;

        public ConfigureContainerAdapter(Action<HostBuilderContext, TContainerBuilder> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder)
        {
            _action(hostContext, (TContainerBuilder)containerBuilder);
        }
    }
}