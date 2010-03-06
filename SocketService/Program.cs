using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Configuration;

namespace SuperSocket.SocketService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        static void Main(string[] args)
		{
            if (args != null && args.Length > 0)
            {
                if (args[0].Equals("-c", StringComparison.OrdinalIgnoreCase))
                {
                    LogUtil.Setup(new ELLogger());

                    SocketServiceConfig serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
                    if (!SocketServerManager.Initialize(serverConfig))
                        return;

                    if (!SocketServerManager.Start(serverConfig))
                        SocketServerManager.Stop();

                    Console.WriteLine("The server has been started!");

                    while (Console.Read() != (int)'q')
                    {
                        continue;
                    }

                    return;
                }
            }

            ServiceBase[] ServicesToRun;

            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
            //
            ServicesToRun = new ServiceBase[] { new MainService() };

            ServiceBase.Run(ServicesToRun);
		}
	}
}