using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Protocol;

namespace TelnetServer_ProcessRequest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start the server!");

            Console.ReadKey();
            Console.WriteLine();

            var appServer = new AppServer();

            var serverConfig = new ServerConfig
            {
                Port = 2012 //set the listening port
            };

            //Setup the appServer
            if (!appServer.Setup(serverConfig, SocketServerFactory.Instance, logFactory: new ConsoleLogFactory()))
            {
                Console.WriteLine("Failed to setup!");
                Console.ReadKey();
                return;
            }

            appServer.NewSessionConnected += new Action<AppSession>(appServer_NewSessionConnected);
            appServer.RequestHandler += new RequestHandler<AppSession, SuperSocket.SocketBase.Protocol.StringRequestInfo>(appServer_RequestHandler);

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

            //Stop the appServer
            appServer.Stop();

            Console.WriteLine("The server was stopped!");
            Console.ReadKey();
        }

        static void appServer_RequestHandler(AppSession session, StringRequestInfo requestInfo)
        {
            switch (requestInfo.Key.ToLower())
            {
                case("ECHO"):
                    session.Send(requestInfo.Data);
                    break;

                case ("ADD"):
                    session.Send(requestInfo.Parameters.Select(p => Convert.ToInt32(p)).Sum().ToString());
                    break;

                case ("MULT"):

                    var result = 1;

                    foreach (var factor in requestInfo.Parameters.Select(p => Convert.ToInt32(p)))
                    {
                        result *= factor;
                    }

                    session.Send(result.ToString());
                    break;
            }
        }

        static void appServer_NewSessionConnected(AppSession session)
        {
            session.Send("Welcome to SuperSocket Telnet Server");
        }
    }
}
