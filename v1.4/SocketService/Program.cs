using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;


namespace SuperSocket.SocketService
{
    static partial class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            LogUtil.Setup();

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
                    Console.WriteLine();
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
            SocketServiceConfig serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
            {
                Console.WriteLine("Failed to initialize SuperSocket server! Please check error log for more information!");
                return;
            }

            if (!SocketServerManager.Start())
            {
                Console.WriteLine("Failed to start SuperSocket server! Please check error log for more information!");
                SocketServerManager.Stop();
                return;
            }

            Console.WriteLine("The server has been started! Press key 'q' to stop the server.");

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Console.WriteLine();
                continue;
            }

            SocketServerManager.Stop();

            Console.WriteLine();
            Console.WriteLine("The server has been stopped!");
        }

        static void RunAsService()
        {
            ServiceBase[] servicesToRun;

            servicesToRun = new ServiceBase[] { new MainService() };

            ServiceBase.Run(servicesToRun);
        }
    }
}