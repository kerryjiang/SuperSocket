using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.ServiceProcess;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
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
            if ((!Platform.IsMono && !Environment.UserInteractive)//Windows Service
                || (Platform.IsMono && !AppDomain.CurrentDomain.FriendlyName.Equals(Path.GetFileName(Assembly.GetEntryAssembly().CodeBase))))//MonoService
            {
                RunAsService();
                return;
            }

            string exeArg = string.Empty;

            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Welcome to SuperSocket SocketService!");

                Console.WriteLine("Please press a key to continue...");
                Console.WriteLine("-[r]: Run this application as a console application;");
                Console.WriteLine("-[i]: Install this application as a Windows Service;");
                Console.WriteLine("-[u]: Uninstall this Windows Service application;");

                while (true)
                {
                    exeArg = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();

                    if (Run(exeArg, null))
                        break;
                }
            }
            else
            {
                exeArg = args[0];

                if (!string.IsNullOrEmpty(exeArg))
                    exeArg = exeArg.TrimStart('-');

                Run(exeArg, args);
            }
        }

        private static bool Run(string exeArg, string[] startArgs)
        {
            switch (exeArg.ToLower())
            {
                case ("i"):
                    SelfInstaller.InstallMe();
                    return true;

                case ("u"):
                    SelfInstaller.UninstallMe();
                    return true;

                case ("r"):
                    RunAsConsole();
                    return true;

                case ("c"):
                    RunAsController(startArgs);
                    return true;

                default:
                    Console.WriteLine("Invalid argument!");
                    return false;
            }
        }

        private static bool setConsoleColor;

        static void CheckCanSetConsoleColor()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.ResetColor();
                setConsoleColor = true;
            }
            catch
            {
                setConsoleColor = false;
            }
        }

        private static void SetConsoleColor(ConsoleColor color)
        {
            if (setConsoleColor)
                Console.ForegroundColor = color;
        }

        private static Dictionary<string, ControlCommand> m_CommandHandlers = new Dictionary<string, ControlCommand>(StringComparer.OrdinalIgnoreCase);

        private static void AddCommand(string name, string description, Func<IBootstrap, string[], bool> handler)
        {
            var command = new ControlCommand
            {
                Name = name,
                Description = description,
                Handler = handler
            };

            m_CommandHandlers.Add(command.Name, command);
        }

        static void RunAsConsole()
        {
            Console.WriteLine("Welcome to SuperSocket SocketService!");

            CheckCanSetConsoleColor();

            Console.WriteLine("Initializing...");

            IBootstrap bootstrap = BootstrapFactory.CreateBootstrap();

            if (!bootstrap.Initialize())
            {
                SetConsoleColor(ConsoleColor.Red);

                Console.WriteLine("Failed to initialize SuperSocket ServiceEngine! Please check error log for more information!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Starting...");

            var result = bootstrap.Start();

            Console.WriteLine("-------------------------------------------------------------------");

            foreach (var server in bootstrap.AppServers)
            {
                if (server.State == ServerState.Running)
                {
                    SetConsoleColor(ConsoleColor.Green);
                    Console.WriteLine("- {0} has been started", server.Name);
                }
                else
                {
                    SetConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("- {0} failed to start", server.Name);
                }
            }

            Console.ResetColor();
            Console.WriteLine("-------------------------------------------------------------------");

            switch(result)
            {
                case(StartResult.None):
                    SetConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("No server is configured, please check you configuration!");
                    Console.ReadKey();
                    return;

                case(StartResult.Success):
                    Console.WriteLine("The SuperSocket ServiceEngine has been started!");
                    break;

                case (StartResult.Failed):
                    SetConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("Failed to start the SuperSocket ServiceEngine! Please check error log for more information!");
                    Console.ReadKey();
                    return;

                case (StartResult.PartialSuccess):
                    SetConsoleColor(ConsoleColor.Red);
                    Console.WriteLine("Some server instances were started successfully, but the others failed! Please check error log for more information!");
                    break;
            }

            Console.ResetColor();
            Console.WriteLine("Enter key 'quit' to stop the ServiceEngine.");

            RegisterCommands();

            ReadConsoleCommand(bootstrap);

            bootstrap.Stop();

            Console.WriteLine();
            Console.WriteLine("The SuperSocket ServiceEngine has been stopped!");
        }

        private static void RegisterCommands()
        {
            AddCommand("List", "List all server instances", ListCommand);
            AddCommand("Start", "Start a server instance: Start {ServerName}", StartCommand);
            AddCommand("Stop", "Stop a server instance: Stop {ServerName}", StopCommand);
        }

        private static void RunAsController(string[] arguments)
        {
            if (arguments == null || arguments.Length < 2)
            {
                Console.WriteLine("Invalid arguments!");
                return;
            }

            var config = ConfigurationManager.GetSection("superSocket") as IConfigurationSource;

            if (config == null)
            {
                Console.WriteLine("SuperSocket configiration is required!");
                return;
            }

            var clientChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(clientChannel, false);

            IBootstrap bootstrap = null;

            try
            {
                var remoteBootstrapUri = string.Format("ipc://SuperSocket.Bootstrap[{0}]/Bootstrap.rem", Math.Abs(AppDomain.CurrentDomain.BaseDirectory.GetHashCode()));
                bootstrap = (IBootstrap)Activator.GetObject(typeof(IBootstrap), remoteBootstrapUri);
            }
            catch (RemotingException)
            {
                if (config.Isolation != IsolationMode.Process)
                {
                    Console.WriteLine("Error: the SuperSocket engine has not been started!");
                    return;
                }
            }

            RegisterCommands();

            var cmdName = arguments[1];

            ControlCommand cmd;

            if (!m_CommandHandlers.TryGetValue(cmdName, out cmd))
            {
                Console.WriteLine("Unknown command");
                return;
            }

            try
            {
                if (cmd.Handler(bootstrap, arguments.Skip(1).ToArray()))
                    Console.WriteLine("Ok");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed. " + e.Message);
            }
        }

        static bool ListCommand(IBootstrap bootstrap, string[] arguments)
        {
            foreach (var s in bootstrap.AppServers)
            {
                Console.WriteLine("{0} - {1}", s.Name, s.State);
            }

            return false;
        }

        static bool StopCommand(IBootstrap bootstrap, string[] arguments)
        {
            var name = arguments[1];

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Server name is required!");
                return false;
            }

            var server = bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (server == null)
            {
                Console.WriteLine("The server was not found!");
                return false;
            }

            server.Stop();

            return true;
        }

        static bool StartCommand(IBootstrap bootstrap, string[] arguments)
        {
            var name = arguments[1];

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Server name is required!");
                return false;
            }

            var server = bootstrap.AppServers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (server == null)
            {
                Console.WriteLine("The server was not found!");
                return false;
            }

            server.Start();

            return true;
        }

        static void ReadConsoleCommand(IBootstrap bootstrap)
        {
            var line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                ReadConsoleCommand(bootstrap);
                return;
            }

            if ("quit".Equals(line, StringComparison.OrdinalIgnoreCase))
                return;

            var cmdArray = line.Split(' ');

            ControlCommand cmd;

            if (!m_CommandHandlers.TryGetValue(cmdArray[0], out cmd))
            {
                Console.WriteLine("Unknown command");
                ReadConsoleCommand(bootstrap);
                return;
            }

            try
            {
                if(cmd.Handler(bootstrap, cmdArray))
                    Console.WriteLine("Ok");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed. " + e.Message);
            }

            ReadConsoleCommand(bootstrap);
        }

        static void RunAsService()
        {
            ServiceBase[] servicesToRun;

            servicesToRun = new ServiceBase[] { new MainService() };

            ServiceBase.Run(servicesToRun);
        }
    }
}