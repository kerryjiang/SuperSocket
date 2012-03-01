using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;

namespace SuperSocket.Management.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex m_SigleInstanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            m_SigleInstanceMutex = new Mutex(true, AppDomain.CurrentDomain.FriendlyName, out createdNew);

            if (createdNew)
                base.OnStartup(e);
            else
                this.Shutdown();
        }
    }
}
