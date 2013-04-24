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

        private const string m_PortNameTemplate = "SuperSocket.Agent[{0}[{1}]]";

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

            var portName = string.Format(m_PortNameTemplate, Name, Guid.NewGuid().ToString().GetHashCode());
            var args = new string[] { Name, portName };

            var startInfo = new ProcessStartInfo(Path.Combine(currentDomain.BaseDirectory, "SuperSocket.Agent.exe"), string.Join(" ", args.Select(a => "\"" + a + "\"").ToArray()));
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = workingDir;
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
                        var line = output.ReadLine();
                        Console.WriteLine(portName + ":" + line);
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

            m_ServerTag = string.Format("{0}/{1}:{2}", Name, m_WorkingProcess.ProcessName, m_WorkingProcess.Id);

            return appServer;
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            m_WorkingProcess = null;
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
                finally
                {
                    var waitExitRound = 0;

                    while (!m_WorkingProcess.HasExited && waitExitRound < 5)
                    {
                        if (m_WorkingProcess.WaitForExit(1000))
                            break;

                        waitExitRound++;
                    }

                    OnStopped();
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
