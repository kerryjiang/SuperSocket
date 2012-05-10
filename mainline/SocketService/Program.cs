using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;


namespace SuperSocket.SocketService
{
    static partial class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                RunAsService();
                return;
            }

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

                Console.WriteLine("Invalid argument!");
                return;
            }

            Console.WriteLine("Press any key to start server!");
            Console.ReadKey();
            Console.WriteLine();
            RunAsConsole();
            Console.ReadKey();
        }

        static void RunAsConsole()
        {
            IBootstrap bootstrap = new DefaultBootstrap();

            SocketServiceConfig serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!bootstrap.Initialize(serverConfig))
            {
                Console.WriteLine("Failed to initialize SuperSocket server! Please check error log for more information!");
                return;
            }

            var result = bootstrap.Start();

            foreach (var server in bootstrap.AppServers)
            {
                if (server.IsRunning)
                    Console.WriteLine("- {0} has been started");
                else
                    Console.WriteLine("- {0} failed to start");
            }

            switch(result)
            {
                case(StartResult.None):
                    Console.WriteLine("No server is configured, please check you configuration!");
                    break;

                case(StartResult.Success):
                    Console.WriteLine("The server has been started!");
                    break;

                case (StartResult.Failed):
                    Console.WriteLine("Failed to start SuperSocket server! Please check error log for more information!");
                    break;

                case (StartResult.PartialSuccess):
                    Console.WriteLine("Some server instances were started successfully, but the others failed to start! Please check error log for more information!");
                    break;
            }

            Console.WriteLine("Press key 'q' to stop the server.");

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Console.WriteLine();
                continue;
            }

            bootstrap.Stop();

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