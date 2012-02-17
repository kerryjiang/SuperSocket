using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperSocket.Common.Logging;

namespace SuperSocket.SuperSocketRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("SuperSocketRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 100;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            LogFactoryProvider.Initialize();

            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;

            if (!SocketServerManager.Initialize(serverConfig, ResolveServerConfig))
            {
                Trace.WriteLine("Failed to initialize SuperSocket!", "Error");
                return false;
            }

            if (!SocketServerManager.Start())
            {
                Trace.WriteLine("Failed to start SuperSocket!", "Error");
                return false;
            }

            return base.OnStart();
        }

        private IServerConfig ResolveServerConfig(IServerConfig serverConfig)
        {
            var config = new ServerConfig();
            serverConfig.CopyPropertiesTo(config);

            if (serverConfig.Port > 0)
            {
                var endPointKey = serverConfig.Name + "_" + serverConfig.Port;
                var instanceEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[endPointKey];
                if (instanceEndpoint == null)
                {
                    Trace.WriteLine(string.Format("Failed to find Input Endpoint configuration {0}!", endPointKey), "Error");
                    return serverConfig;
                }

                var ipEndpoint = instanceEndpoint.IPEndpoint;
                config.Ip = ipEndpoint.Address.ToString();
                config.Port = ipEndpoint.Port;
            }

            if(config.Listeners != null && config.Listeners.Any())
            {
                var listeners = config.Listeners.ToArray();

                var newListeners = new List<ListenerConfig>(listeners.Length);

                for (var i = 0; i < listeners.Length; i++)
                {
                    var listener = listeners[i];

                    var endPointKey = serverConfig.Name + "_" + listener.Port;
                    var instanceEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[endPointKey];
                    if (instanceEndpoint == null)
                    {
                        Trace.WriteLine(string.Format("Failed to find Input Endpoint configuration {0}!", endPointKey), "Error");
                        return serverConfig;
                    }

                    var newListener = new ListenerConfig();
                    newListener.Ip = instanceEndpoint.IPEndpoint.Address.ToString();
                    newListener.Port = instanceEndpoint.IPEndpoint.Port;
                    newListener.Backlog = listener.Backlog;

                    newListeners.Add(newListener);
                }

                config.Listeners = newListeners;
            }

            return config;
        }

        public override void OnStop()
        {
            SocketServerManager.Stop();
            base.OnStop();
        }
    }
}
