using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;

namespace SampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start server!");

            LogUtil.Setup();

            SocketServiceConfig serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
            {
                Console.WriteLine("Failed to initialize SuperSocket server! Please check error log for more information!");
                Console.ReadLine();
                return;
            }

            if (!SocketServerManager.Start())
            {
                Console.WriteLine("Failed to start SuperSocket server! Please check error log for more information!");
                SocketServerManager.Stop();
                Console.ReadLine();
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
            Console.ReadLine();
        }
    }
}
