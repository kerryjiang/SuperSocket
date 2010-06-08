using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Configuration;

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
			serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.ServiceName = ConfigurationManager.AppSettings["ServiceName"];

			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}
	}
}