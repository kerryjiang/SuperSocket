using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperSocket.SocketBase;

namespace SampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start server!");

            var bootstrap = new DefaultBootstrap();
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;

            if (!bootstrap.Initialize(serverConfig))
            {
                Console.WriteLine("Failed to initialize SuperSocket server! Please check error log for more information!");
                return;
            }

            var result = bootstrap.Start();

            switch (result)
            {
                case (StartResult.None):
                    Console.WriteLine("No server is configured, please check you configuration!");
                    break;

                case (StartResult.Success):
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
    }
}
