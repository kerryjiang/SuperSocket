using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace SuperSocket.Agent
{
    /// <summary>
    /// Entrance class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if(args.Length != 3)
                throw new ArgumentException("Arguments number doesn't match!", "args");

            var name = args[0];

            if(string.IsNullOrEmpty(name))
                throw new Exception("Name cannot be null or empty.");

            name = name.Trim('"');

            var channelPort = args[1];

            if (string.IsNullOrEmpty(channelPort))
                throw new Exception("Channel port cannot be null or empty.");

            channelPort = channelPort.Trim('"');
            channelPort = string.Format(channelPort, Process.GetCurrentProcess().Id);

            var root = args[2];

            if (string.IsNullOrEmpty(root))
                throw new Exception("Root cannot be null or empty.");

            AppDomain.CurrentDomain.SetData("APPBASE", root);
            AppDomain.CurrentDomain.SetData(typeof(IsolationMode).Name, IsolationMode.Process);

            try
            {
                var serverChannel = new IpcServerChannel("IpcAgent", channelPort);
                var clientChannel = new IpcClientChannel();
                ChannelServices.RegisterChannel(serverChannel, false);
                ChannelServices.RegisterChannel(clientChannel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(WorkItemAgent), "WorkItemAgent.rem", WellKnownObjectMode.Singleton);
                Console.WriteLine("Ok");

                var line = Console.ReadLine();

                while (!"quit".Equals(line, StringComparison.OrdinalIgnoreCase))
                {
                    line = Console.ReadLine();
                }
            }
            catch
            {
                Console.WriteLine("Failed");
            }
        }
    }
}
