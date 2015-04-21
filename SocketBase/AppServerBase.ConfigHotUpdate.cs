using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase
{
    interface IConfigValueChangeNotifier
    {
        bool Notify(string newValue);
    }

    class ConfigValueChangeNotifier : IConfigValueChangeNotifier
    {
        Func<string, bool> m_Handler;

        public ConfigValueChangeNotifier(Func<string, bool> handler)
        {
            m_Handler = handler;
        }

        public bool Notify(string newValue)
        {
            return m_Handler(newValue);
        }
    }
    class ConfigValueChangeNotifier<TConfigOption> : IConfigValueChangeNotifier
        where TConfigOption : ConfigurationElement, new()
    {
        Func<TConfigOption, bool> m_Handler;

        public ConfigValueChangeNotifier(Func<TConfigOption, bool> handler)
        {
            m_Handler = handler;
        }
        public bool Notify(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                return m_Handler(default(TConfigOption));
            else
                return m_Handler(ConfigurationExtension.DeserializeChildConfig<TConfigOption>(newValue));
        }
    }

    public abstract partial class AppServerBase<TAppSession, TRequestInfo>
        where TRequestInfo : class, IRequestInfo
        where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
    {
        private Dictionary<string, IConfigValueChangeNotifier> m_ConfigUpdatedNotifiers = new Dictionary<string, IConfigValueChangeNotifier>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers the configuration option value handler, it is used for reading configuration value and reload it after the configuration is changed;
        /// </summary>
        /// <typeparam name="TConfigOption">The type of the configuration option.</typeparam>
        /// <param name="config">The server configuration.</param>
        /// <param name="name">The changed config option's name.</param>
        /// <param name="handler">The handler.</param>
        protected bool RegisterConfigHandler<TConfigOption>(IServerConfig config, string name, Func<TConfigOption, bool> handler)
            where TConfigOption : ConfigurationElement, new()
        {
            var notifier = new ConfigValueChangeNotifier<TConfigOption>(handler);
            m_ConfigUpdatedNotifiers.Add(name, notifier);
            return notifier.Notify(config.Options.GetValue(name));
        }

        /// <summary>
        /// Registers the configuration option value handler, it is used for reading configuration value and reload it after the configuration is changed;
        /// </summary>
        /// <param name="config">The server configuration.</param>
        /// <param name="name">The changed config option name.</param>
        /// <param name="handler">The handler.</param>
        protected bool RegisterConfigHandler(IServerConfig config, string name, Func<string, bool> handler)
        {
            var notifier = new ConfigValueChangeNotifier(handler);
            m_ConfigUpdatedNotifiers.Add(name, notifier);
            return notifier.Notify(config.OptionElements.GetValue(name));
        }

        int CheckConfigOptionsChange(NameValueCollection oldOptions, NameValueCollection newOptions)
        {
            var changed = 0;

            if (oldOptions == null && newOptions == null)
                return changed;

            var oldOptionsDict = oldOptions == null
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : Enumerable.Range(0, oldOptions.Count)
                        .Select(i => new KeyValuePair<string, string>(oldOptions.GetKey(i), oldOptions.Get(i)))
                        .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

            foreach(var key in newOptions.AllKeys)
            {
                var newValue = newOptions[key];

                var oldValue = string.Empty;

                if (oldOptionsDict.TryGetValue(key, out oldValue))
                    oldOptionsDict.Remove(key);

                if (string.Compare(newValue, oldValue) == 0)
                    continue;

                NotifyConfigUpdated(key, newValue);
                changed++;
            }

            if (oldOptionsDict.Count > 0)
            {
                foreach (var p in oldOptionsDict)
                {
                    NotifyConfigUpdated(p.Key, string.Empty);
                    changed++;
                }
            }

            return changed;
        }

        private void NotifyConfigUpdated(string key, string newValue)
        {
            IConfigValueChangeNotifier notifier;

            if (!m_ConfigUpdatedNotifiers.TryGetValue(key, out notifier))
                return;

            try
            {
                if (!notifier.Notify(newValue))
                    throw new Exception("returned false in the handling logic");
            }
            catch (Exception e)
            {
                Logger.Error("Failed to handle custom configuration reading, name: " + key, e);
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
