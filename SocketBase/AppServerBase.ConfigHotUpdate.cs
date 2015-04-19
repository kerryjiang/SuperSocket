using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    interface IConfigValueChangeNotifier
    {
        void Notify(string newValue);
    }

    class ConfigValueChangeNotifier : IConfigValueChangeNotifier
    {
        Action<string> m_Handler;

        public ConfigValueChangeNotifier(Action<string> handler)
        {
            m_Handler = handler;
        }

        public void Notify(string newValue)
        {
            m_Handler(newValue);
        }
    }
    class ConfigValueChangeNotifier<TConfigOption> : IConfigValueChangeNotifier
        where TConfigOption : ConfigurationElement, new()
    {
        Action<TConfigOption> m_Handler;

        public ConfigValueChangeNotifier(Action<TConfigOption> handler)
        {
            m_Handler = handler;
        }
        public void Notify(string newValue)
        {
            m_Handler(ConfigurationExtension.DeserializeChildConfig<TConfigOption>(newValue));
        }
    }

    public abstract partial class AppServer<TAppSession, TPackageInfo, TKey> : IAppServer<TAppSession, TPackageInfo>, IRawDataProcessor<TAppSession>, IRequestHandler<TPackageInfo>, ISocketServerAccessor, IStatusInfoSource, IRemoteCertificateValidator, IActiveConnector, ISessionRegister, ISystemEndPoint, IDisposable
        where TPackageInfo : class, IPackageInfo<TKey>
        where TAppSession : AppSession<TAppSession, TPackageInfo, TKey>, IAppSession, new()
    {
        private Dictionary<string, IConfigValueChangeNotifier> m_ConfigUpdatedNotifiers = new Dictionary<string, IConfigValueChangeNotifier>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers the configuration option value handler, it is used for reading configuration value and reload it after the configuration is changed;
        /// </summary>
        /// <typeparam name="TConfigOption">The type of the configuration option.</typeparam>
        /// <param name="name">The changed config option's name.</param>
        /// <param name="handler">The handler.</param>
        protected void RegisterConfigHandler<TConfigOption>(string name, Action<TConfigOption> handler)
            where TConfigOption : ConfigurationElement, new()
        {
            m_ConfigUpdatedNotifiers.Add(name, new ConfigValueChangeNotifier<TConfigOption>(handler));
        }

        /// <summary>
        /// Registers the configuration option value handler, it is used for reading configuration value and reload it after the configuration is changed;
        /// </summary>
        /// <param name="name">The changed config option name.</param>
        /// <param name="handler">The handler.</param>
        protected void RegisterConfigHandler(string name, Action<string> handler)
        {
            m_ConfigUpdatedNotifiers.Add(name, new ConfigValueChangeNotifier(handler));
        }

        void CheckConfigOptionsChange(NameValueCollection oldOptions, NameValueCollection newOptions)
        {
            foreach(var key in newOptions.AllKeys)
            {
                var newValue = newOptions[key];
                var oldValue = oldOptions[key];

                if (string.Compare(newValue, oldValue) == 0)
                    continue;

                IConfigValueChangeNotifier notifier;

                if (!m_ConfigUpdatedNotifiers.TryGetValue(key, out notifier))
                    continue;

                try
                {
                    notifier.Notify(newValue);
                }
                catch(Exception e)
                {
                    Logger.Error("Failed to handle custom configuration reading, name: " + key, e);
                }
            }
        }

        void IWorkItemBase.ReportPotentialConfigChange(IServerConfig config)
        {
            var oldConfig = this.Config;

            CheckConfigOptionsChange(oldConfig.Options, config.Options);
            CheckConfigOptionsChange(oldConfig.OptionElements, config.OptionElements);

            var updatableConfig = oldConfig as ServerConfig;

            if (updatableConfig == null)
                return;

            config.CopyPropertiesTo(p => p.GetCustomAttributes(typeof(HotUpdateAttribute), true).Length > 0, updatableConfig);
        }
    }
}
