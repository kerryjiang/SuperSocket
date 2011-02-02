using System;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using SuperSocket.Common;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace SuperSocket.SocketBase.ConnectionFilter
{
    public abstract class TextFileSourceFilter : IConnectionFilter, IAsyncRunner
    {
        private FileSystemWatcher m_FileSourceWatcher;
        private string m_FullFilePath;
        private HashSet<string> m_IpLibrary = new HashSet<string>();
        private long m_LastUpdatedTicks = 0;
        
        #region IConnectionFilter implementation
        
        public virtual bool Initialize(string name, NameValueCollection options)
        {
            Name = name;
            string filePath = options.GetValue("filePath");
            if(string.IsNullOrEmpty(filePath))
            {
                LogUtil.LogError(string.Format("The attribute 'filePath' is requred for connection filter: {0}!", filePath));
                return false;
            }
            
            if(!File.Exists(filePath))
            {
                LogUtil.LogError(string.Format("The file specified for connection filter: {0} does not exist!", filePath));
                return false;
            }
            
            try
            {
                m_FullFilePath = Path.GetFullPath(filePath);
                var parentDir = Path.GetDirectoryName(m_FullFilePath);
                m_FileSourceWatcher = new FileSystemWatcher(parentDir);
            }
            catch(Exception e)
            {
                LogUtil.LogError(e);
                return false;
            }
            m_FileSourceWatcher.Error += HandleFileSourceWatcherError;
            m_FileSourceWatcher.EnableRaisingEvents = false;
            m_FileSourceWatcher.IncludeSubdirectories = true;
            m_FileSourceWatcher.Created += HandleFileSourceWatcherCreated;
            m_FileSourceWatcher.Changed += HandleFileSourceWatcherChanged;
            m_FileSourceWatcher.EnableRaisingEvents = true;
            
            return true;
        }
        
        bool IsFileChanged(string filePath, out long lastModifiedTimeTicks)
        {
            lastModifiedTimeTicks = File.GetLastWriteTime(filePath).Ticks;
            return lastModifiedTimeTicks > m_LastUpdatedTicks;
        }

        void HandleFileSourceWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if(!m_FullFilePath.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase))
                return;
            
            long lastModifiedTimeTicks;
            if(!IsFileChanged(e.FullPath, out lastModifiedTimeTicks))
                return;
            
            this.ExecuteAsync(w => ProcessUpdatedFile(e.FullPath, lastModifiedTimeTicks));
        }

        void HandleFileSourceWatcherCreated(object sender, FileSystemEventArgs e)
        {
            if(!m_FullFilePath.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase))
                return;
            
            long lastModifiedTimeTicks;
            if(!IsFileChanged(e.FullPath, out lastModifiedTimeTicks))
                return;
            
            this.ExecuteAsync(w => ProcessUpdatedFile(e.FullPath, lastModifiedTimeTicks));
        }
        
        void ProcessUpdatedFile(string filePath, long lastModifiedTimeTicks)
        {
            using(StreamReader reader = new StreamReader(filePath, Encoding.UTF8, true))
            {
                HashSet<string> newIpLibrary = new HashSet<string>();
                
                while(reader.Peek() > 0)
                {
                    string line = reader.ReadLine();
                    if(!string.IsNullOrEmpty(line))
                        newIpLibrary.Add(line);
                }
                
                Interlocked.Exchange(ref m_IpLibrary, newIpLibrary);
                Interlocked.Exchange(ref m_LastUpdatedTicks, lastModifiedTimeTicks);
            }
        }

        void HandleFileSourceWatcherError(object sender, ErrorEventArgs e)
        {
            LogUtil.LogError("Connection filer's file watcher error!", e.GetException());
        }

        public abstract bool AllowConnect(IPEndPoint remoteAddress);

        public string Name { get; private set; }
        
        #endregion
        
        protected bool Contains(string ipAddress)
        {
            return m_IpLibrary.Contains(ipAddress);
        }
    }
}