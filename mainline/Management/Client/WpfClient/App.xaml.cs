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
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                InitailizeConfig();
                base.OnStartup(e);
            }
            else
            {
                this.Shutdown();
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //MessageBox.Show(e.ExceptionObject.ToString());
        }

        private const string m_ConfigFile = "Client.config";

        private void InitailizeConfig()
        {
            if (!File.Exists(m_ConfigFile))
            {
                var config = new ClientAppConfig();
                config.Servers = new ServerConfig[0];
                ClientConfig = config;
                return;
            }

            var configXml = File.ReadAllText(m_ConfigFile, Encoding.UTF8);
            ClientConfig = configXml.XmlDeserialize<ClientAppConfig>();
        }

        internal static void SaveConfig()
        {
            ClientConfig.XmlSerialize(m_ConfigFile);
        }
    }
}
