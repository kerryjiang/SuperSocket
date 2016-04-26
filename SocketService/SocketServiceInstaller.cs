using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SuperSocket.SocketService
{
    [RunInstaller(true)]
    public partial class SocketServiceInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public SocketServiceInstaller()
        {
            InitializeComponent();

            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = ConfigurationManager.AppSettings["ServiceName"];

            var serviceDisplayName = ConfigurationManager.AppSettings["ServiceDisplayName"];
            if (!string.IsNullOrEmpty(serviceDisplayName))
                serviceInstaller.DisplayName = serviceDisplayName;

            var serviceDescription = ConfigurationManager.AppSettings["ServiceDescription"];
            if (!string.IsNullOrEmpty(serviceDescription))
                serviceInstaller.Description = serviceDescription;

            var servicesDependedOn = new List<string> { "tcpip" };
            var servicesDependedOnConfig = ConfigurationManager.AppSettings["ServicesDependedOn"];

            if (!string.IsNullOrEmpty(servicesDependedOnConfig))
                servicesDependedOn.AddRange(servicesDependedOnConfig.Split(new char[] { ',', ';' }));

            serviceInstaller.ServicesDependedOn = servicesDependedOn.ToArray();

            var serviceStartAfterInstall = ConfigurationManager.AppSettings["ServiceStartAfterInstall"];
            if (!string.IsNullOrEmpty(serviceStartAfterInstall) && serviceStartAfterInstall.ToLower() == "true")
                this.AfterInstall += new InstallEventHandler(ProjectInstaller_AfterInstall);

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        private void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            ServiceController sc = new ServiceController(serviceInstaller.ServiceName);
            sc.Start();
        }
    }
}
