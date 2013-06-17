using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using SuperSocket.Management.AgentClient.Config;
using SuperSocket.Management.AgentClient.ViewModel;

namespace SuperSocket.Management.AgentClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex m_SigleInstanceMutex;

        internal static AgentConfig Config { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            m_SigleInstanceMutex = new Mutex(true, AppDomain.CurrentDomain.FriendlyName, out createdNew);

            if (!createdNew)
                this.Shutdown();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel(AgentConfig.Load());
            this.MainWindow = mainWindow;
            mainWindow.Show();

            base.OnStartup(e);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //MessageBox.Show(e.ExceptionObject.ToString());
        }
    }
}
