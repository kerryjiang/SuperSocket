using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    class ProcessAppServer : IsolationAppServer
    {
        private const string m_AgentUri = "ipc://{0}/WorkItemAgent.rem";

        private const string m_PortNameTemplate = "{0}[SuperSocket.Agent:{1}]";

        private Process m_WorkingProcess;

        private string m_ServerTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessAppServer" /> class.
        /// </summary>
        /// <param name="serverTypeName">Name of the server type.</param>
        public ProcessAppServer(string serverTypeName)
            : base(serverTypeName)
        {

        }

        protected override IWorkItemBase Start()
        {
            var currentDomain = AppDomain.CurrentDomain;
            var workingDir = Path.Combine(Path.Combine(currentDomain.BaseDirectory, WorkingDir), Name);

            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);

            var portName = string.Format(m_PortNameTemplate, Name, "{0}");
            var args = new string[] { Name, portName, workingDir };

            var startInfo = new ProcessStartInfo("SuperSocket.Agent.exe", string.Join(" ", args.Select(a => "\"" + a + "\"").ToArray()));
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = currentDomain.BaseDirectory;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            try
            {
                m_WorkingProcess = Process.Start(startInfo);
            }
            catch
            {
                return null;
            }

            portName = string.Format(portName, m_WorkingProcess.Id);

            var output = m_WorkingProcess.StandardOutput;

            var startResult = output.ReadLine();

            if (!"Ok".Equals(startResult, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Task.Factory.StartNew(() =>
                {
                    while (!output.EndOfStream)
                    {
                        output.ReadLine();
                    }
                }, TaskCreationOptions.LongRunning);

            var remoteUri = string.Format(m_AgentUri, portName);
            var appServer = (IRemoteWorkItem)Activator.GetObject(typeof(IRemoteWorkItem), remoteUri);

            var ret = appServer.Setup(ServerTypeName, "ipc://" + ProcessBootstrap.BootstrapIpcPort + "/Bootstrap.rem", currentDomain.BaseDirectory, ServerConfig, Factories);

            if (!ret)
            {
                ShutdownProcess();
                return null;
            }

            ret = appServer.Start();

            if (!ret)
            {
                ShutdownProcess();
                return null;
            }

            m_ServerTag = portName;

            m_WorkingProcess.EnableRaisingEvents = true;
            m_WorkingProcess.Exited += new EventHandler(m_WorkingProcess_Exited);

            return appServer;
        }

        void m_WorkingProcess_Exited(object sender, EventArgs e)
        {
            OnStopped();
        }

        protected override void OnStopped()
        {
            var unexpectedShutdown = (State == ServerState.Running);

            base.OnStopped();
            m_WorkingProcess = null;

            if (unexpectedShutdown)
            {
                //auto restart if meet a unexpected shutdown
                ((IWorkItemBase)this).Start();
            }
        }

        private void ShutdownProcess()
        {
            if (m_WorkingProcess != null)
            {
                try
                {
                    m_WorkingProcess.Kill();
                }
                catch
                {

                }
            }
        }

        protected override void Stop()
        {
            ShutdownProcess();
        }

        public override StatusInfoCollection CollectServerStatus(StatusInfoCollection nodeStatus)
        {
            var status = base.CollectServerStatus(nodeStatus);
            status.Tag = m_ServerTag;
            return status;
        }
    }
}
