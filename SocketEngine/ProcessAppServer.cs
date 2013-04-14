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
    class ProcessAppServer : MarshalByRefObject, IWorkItem, IStatusInfoSource, IDisposable
    {
        private string m_ServiceTypeName;

        private const string m_WorkingDir = "InstancesRoot";

        private const string m_AgentUri = "ipc://{0}/WorkItemAgent.rem";

        private const string m_PortNameTemplate = "SuperSocket.Agent[{0}[{1}]]";

        private IServerConfig m_Config;

        private ProviderFactoryInfo[] m_Factories;

        private Process m_WorkingProcess;

        private IRemoteWorkItem m_RemoteWorkItem;

        private string m_ServerTag;

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

            m_Config = config;
            m_Factories = factories;

            State = ServerState.NotStarted;

            return true;
        }

        public bool Start()
        {
            State = ServerState.Starting;

            var currentDomain = AppDomain.CurrentDomain;
            var workingDir = Path.Combine(Path.Combine(currentDomain.BaseDirectory, m_WorkingDir), Name);

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
                State = ServerState.NotStarted;
                return false;
            }

            var output = m_WorkingProcess.StandardOutput;

            var startResult = output.ReadLine();

            if (!"Ok".Equals(startResult, StringComparison.OrdinalIgnoreCase))
            {
                State = ServerState.NotStarted;
                return false;
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
            m_RemoteWorkItem = (IRemoteWorkItem)Activator.GetObject(typeof(IRemoteWorkItem), remoteUri);

            var ret = m_RemoteWorkItem.Setup(m_ServiceTypeName, "ipc://" + ProcessBootstrap.BootstrapIpcPort + "/Bootstrap.rem", currentDomain.BaseDirectory, m_Config, m_Factories);

            if (!ret)
            {
                State = ServerState.NotStarted;
                return false;
            }

            ret = m_RemoteWorkItem.Start();

            State = ret ? ServerState.Running : ServerState.NotStarted;

            m_WorkingProcess.Exited += new EventHandler(m_WorkingProcess_Exited);
            m_ServerTag = string.Format("{0}/{1}:{2}", Name, m_WorkingProcess.ProcessName, m_WorkingProcess.Id);

            return ret;
        }

        void m_WorkingProcess_Exited(object sender, EventArgs e)
        {
            m_RemoteWorkItem = null;
            State = ServerState.NotStarted;
        }

        public ServerState State { get; private set; }

        public void Stop()
        {
            State = ServerState.Stopping;

            try
            {
                if (m_RemoteWorkItem != null)
                    m_RemoteWorkItem.Stop();
            }
            catch
            {
            }
            finally
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

            State = ServerState.NotStarted;
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

        private StatusInfoCollection m_PrevStatus;
        private StatusInfoCollection m_StoppedStatus;

        private StatusInfoCollection GetStoppedStatus()
        {
            if (m_StoppedStatus != null)
            {
                m_StoppedStatus = new StatusInfoCollection();
                m_StoppedStatus.Name = Name;
                m_StoppedStatus.Tag = m_PrevStatus.Tag;
                m_StoppedStatus[ServerStatusInfoMetadata.IsRunning] = false;

                if (m_PrevStatus != null)
                {
                    m_StoppedStatus[ServerStatusInfoMetadata.Listeners] = m_PrevStatus[ServerStatusInfoMetadata.Listeners];
                }
            }

            return m_StoppedStatus;
        }

        StatusInfoCollection IStatusInfoSource.CollectServerStatus(StatusInfoCollection nodeStatus)
        {
            if (m_RemoteWorkItem == null)
            {
                var stoppedStatus = GetStoppedStatus();
                stoppedStatus.CollectedTime = DateTime.Now;
                stoppedStatus.Tag = m_ServerTag;
                return stoppedStatus;
            }

            var currentStatus = m_RemoteWorkItem.CollectServerStatus(nodeStatus);
            m_PrevStatus = currentStatus;
            return currentStatus;
        }

        StatusInfoAttribute[] IStatusInfoSource.GetServerStatusMetadata()
        {
            return m_RemoteWorkItem.GetServerStatusMetadata();
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
                    m_WorkingProcess.Close();
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
