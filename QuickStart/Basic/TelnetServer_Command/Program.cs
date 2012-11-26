using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Logging;

namespace TelnetServer_Command
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start the server!");

            Console.ReadKey();
            Console.WriteLine();

            var appServer = new AppServer();

            //Setup with config model, which provides more options
            var serverConfig = new ServerConfig
            {
                Port = 2012, //set the listening port
                //Other configuration options
                //Mode = SocketMode.Udp,
                //MaxConnectionNumber = 100,
                //...
            };

            //Setup the appServer
            if (!appServer.Setup(serverConfig))
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }

            appServer.NewSessionConnected += new SessionHandler<AppSession>(appServer_NewSessionConnected);

            Console.WriteLine();

            //Try to start the appServer
            if (!appServer.Start())
            {
                Console.WriteLine("Failed to start!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("The server started successfully, press key 'q' to stop it!");

            while (Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine();
                continue;
            }

            Console.WriteLine();
            //Stop the appServer
            appServer.Stop();
            
            Console.WriteLine("The server was stopped!");
        }

        static void appServer_NewSessionConnected(AppSession session)
        {
            session.Send("Welcome to SuperSocket Telnet Server");
        }
    }
}
