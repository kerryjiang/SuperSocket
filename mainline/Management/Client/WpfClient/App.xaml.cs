using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Threading;
using SuperSocket.Management.Client.Config;
using System.IO;
using System.Text;

namespace SuperSocket.Management.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex m_SigleInstanceMutex;

        internal static ClientAppConfig ClientConfig { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            m_SigleInstanceMutex = new Mutex(true, AppDomain.CurrentDomain.FriendlyName, out createdNew);

            if (createdNew)
            {
                InitailizeConfig();
                base.OnStartup(e);
            }
            else
            {
                this.Shutdown();
            }
        }

        private void InitailizeConfig()
        {
            var filePath = "Client.config";

            if (!File.Exists(filePath))
            {
                var config = new ClientAppConfig();

                config.Servers = new ServerConfig[]
                {
                    new ServerConfig
                    {
                        Name = "localhost"
                    }
                };

                config.XmlSerialize(filePath);

                ClientConfig = config;

                return;
            }

            var configXml = File.ReadAllText(filePath, Encoding.UTF8);
            ClientConfig = configXml.XmlDeserialize<ClientAppConfig>();
        }
    }
}
