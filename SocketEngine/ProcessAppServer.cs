using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketEngine
{
    class ProcessAppServer : MarshalByRefObject, IWorkItem, IDisposable
    {
        private string m_ServiceTypeName;

        private const string m_WorkingDir = "InstancesRoot";

        private const string m_AgentUri = "ipc://{0}/WorkItemAgent.rem";

        private Process m_WorkingProcess;

        private IRemoteWorkItem m_RemoteWorkItem;

        public ProcessAppServer(string serviceTypeName)
        {
            m_ServiceTypeName = serviceTypeName;
            State = ServerState.NotInitialized;
        }

        public string Name { get; private set; }

        public bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories)
        {
            State = ServerState.Initializing;

            Name = config.Name;

            var currentDomain = AppDomain.CurrentDomain;
            var workingDir = Path.Combine(Path.Combine(currentDomain.BaseDirectory, m_WorkingDir), Name);

            var portName = "SuperSocket.Agent." + Name;
            var args = new string[] { Name, portName};

            var startInfo = new ProcessStartInfo(Path.Combine(currentDomain.BaseDirectory, "SuperSocket.Agent.exe"), string.Join(" ", args.Select(a => "\"" + a + "\"").ToArray()));
            //startInfo.CreateNoWindow = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.WorkingDirectory = workingDir;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            try
            {
                m_WorkingProcess = Process.Start(startInfo);
            }
            catch
            {
                State = ServerState.NotInitialized;
                return false;
            }

            var output = m_WorkingProcess.StandardOutput;

            var startResult = output.ReadLine();

            Console.WriteLine("Received Line: {0}.", startResult);

            if (!"Ok".Equals(startResult, StringComparison.OrdinalIgnoreCase))
            {
                State = ServerState.NotInitialized;
                return false;
            }

            var remoteUri = string.Format(m_AgentUri, portName);
            m_RemoteWorkItem = (IRemoteWorkItem)Activator.GetObject(typeof(IRemoteWorkItem), remoteUri);
            
            var ret = m_RemoteWorkItem.Setup(m_ServiceTypeName, "ipc://" + ProcessBootstrap.BootstrapIpcPort + "/Bootstrap.rem", currentDomain.BaseDirectory, config, factories);

            State = ret ? ServerState.NotStarted : ServerState.NotInitialized;

            return ret;
        }

        public bool Start()
        {
            State = ServerState.Starting;

            var ret = m_RemoteWorkItem.Start();

            State = ret ? ServerState.Running : ServerState.NotStarted;

            return ret;
        }

        public ServerState State { get; private set; }

        public void Stop()
        {
            m_RemoteWorkItem.Stop();
        }

        public int SessionCount
        {
            get
            {
                if (m_RemoteWorkItem == null)
                    return 0;

                return m_RemoteWorkItem.SessionCount;
            }
        }

        public ServerSummary Summary
        {
            get { throw new NotImplementedException(); }
        }

        public ServerSummary CollectServerSummary(NodeSummary nodeSummary)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass 
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
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
                    m_WorkingProcess = null;
                }
            }
        }

        ~ProcessAppServer()
        {
            Dispose(false);
        }
    }
}
