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
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;

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
            LogUtil.Setup();
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;

            if (!SocketServerManager.Initialize(serverConfig))
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

        public override void OnStop()
        {
            SocketServerManager.Stop();
            base.OnStop();
        }
    }
}
