using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SuperSocket.Common;
using SuperSocket.Management.Server.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Protocol;
using SuperSocket.WebSocket.SubProtocol;
using System.IO;
using System.Threading;

namespace SuperSocket.Management.Server
{
    /// <summary>
    /// Server manager app server
    /// </summary>
    public class ManagementServer : WebSocketServer<ManagementSession>
    {
        private Dictionary<string, UserConfig> m_UsersDict;

        private string[] m_ExcludedServers;

        private FileSystemWatcher m_StatusFileWatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementServer"/> class.
        /// </summary>
        public ManagementServer()
            : base(new BasicSubProtocol<ManagementSession>("ServerManager"))
        {

        }


        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            if (!base.Setup(rootConfig, config))
                return false;

            var users = config.GetChildConfig<UserConfigCollection>("users");

            if (users == null || users.Count <= 0)
            {
                Logger.Error("No user defined");
                return false;
            }

            m_UsersDict = new Dictionary<string, UserConfig>(StringComparer.OrdinalIgnoreCase);

            foreach (var u in users)
            {
                m_UsersDict.Add(u.Name, u);
            }

            m_ExcludedServers = config.Options.GetValue("excludedServers", string.Empty).Split(
                new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            return true;
        }

        private long m_LastModifiedTime = 0;

        protected override void OnStarted()
        {
            m_StatusFileWatcher = new FileSystemWatcher(Path.Combine(Bootstrap.BaseDirectory), "status.bin");
            m_StatusFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            m_StatusFileWatcher.Changed += new FileSystemEventHandler(m_StatusFileWatcher_Changed);
            m_StatusFileWatcher.EnableRaisingEvents = true;

            base.OnStarted();
        }

        void m_StatusFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            m_StatusFileWatcher.EnableRaisingEvents = false;

            if(!FireStatusFileChanged(e))
                m_StatusFileWatcher.EnableRaisingEvents = true;
        }

        private bool FireStatusFileChanged(FileSystemEventArgs e)
        {
            long currentTicks = 0;

            try
            {
                currentTicks = File.GetLastWriteTime(e.FullPath).Ticks;
            }
            catch (Exception exc)
            {
                Logger.Error("One exception was thrown when get the status file's last write time.", exc);
                return false;
            }

            var prevTicks = m_LastModifiedTime;

            if (currentTicks == prevTicks)
                return false;

            //Already updated
            if (Interlocked.CompareExchange(ref m_LastModifiedTime, currentTicks, prevTicks) != prevTicks)
                return false;

            ThreadPool.QueueUserWorkItem(HandleUpdatedStatusFile, e.FullPath);

            return true;
        }

        private void HandleUpdatedStatusFile(object state)
        {
            var filePath = (string)state;

            Thread.Sleep(500);

            NodeStatus nodeStatus;

            try
            {
                nodeStatus = NodeStatus.LoadFrom(filePath);
                Logger.Info(JsonConvert.SerializeObject(nodeStatus, m_IPEndPointConverter));
            }
            catch (Exception exc)
            {
                Logger.Error("One exception was thrown when load the status data file.", exc);
            }
            finally
            {
                m_StatusFileWatcher.EnableRaisingEvents = true;
            }
        }

        protected override void OnStopped()
        {
            m_StatusFileWatcher.EnableRaisingEvents = false;
            m_StatusFileWatcher.Dispose();
            m_StatusFileWatcher = null;

            base.OnStopped();
        }

        private static JsonConverter m_IPEndPointConverter = new ListenersJsonConverter();

        /// <summary>
        /// Jsons the serialize.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public override string JsonSerialize(object target)
        {
            return JsonConvert.SerializeObject(target, m_IPEndPointConverter);
        }
    }
}
