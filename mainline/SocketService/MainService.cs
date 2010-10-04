using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Configuration;

namespace SuperSocket.SocketService
{
    partial class MainService : ServiceBase
    {
        public MainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SocketServiceConfig serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            if (!SocketServerManager.Start(serverConfig))
                SocketServerManager.Stop();
        }

        protected override void OnStop()
        {
            SocketServerManager.Stop();
            base.OnStop();
        }

        protected override void OnShutdown()
        {
            SocketServerManager.Stop();
            base.OnShutdown();
        }
    }
}
