using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace GiantSoft.SocketService
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
			serviceInstaller.ServiceName = "GiantSoft Socket Service";

			Installers.Add(serviceInstaller);
			Installers.Add(processInstaller);
		}
	}
}