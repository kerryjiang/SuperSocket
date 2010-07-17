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
                if (args[0].Equals("-i", StringComparison.OrdinalIgnoreCase))
                {
                    SelfInstaller.InstallMe();
                    return;
                }
                else if (args[0].Equals("-u", StringComparison.OrdinalIgnoreCase))
                {
                    SelfInstaller.UninstallMe();
                    return;
                }
                else if (args[0].Equals("-c", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Press any key to start server!");
                    Console.ReadKey();
                    RunAsConsole();
                }
                else
                {
                    Console.WriteLine(args[0]);
                }
            }
            else
            {
                RunAsService();
            }
        }

        static void RunAsConsole()
        {
            LogUtil.Setup(new ELLogger());

            SocketServiceConfig serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
            {
                Console.WriteLine("Failed to initialize SuperSocket server! Please check error log for more information!");
                return;
            }

            if (!SocketServerManager.Start(serverConfig))
            {
                Console.WriteLine("Failed to start SuperSocket server! Please check error log for more information!");
                SocketServerManager.Stop();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("The server has been started! Press key 'q' to stop the server.");

            while (Console.ReadKey().Key != ConsoleKey.Q)
                continue;

            SocketServerManager.Stop();

            Console.WriteLine();
            Console.WriteLine("The server has been stopped!");
        }

        static void RunAsService()
        {
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