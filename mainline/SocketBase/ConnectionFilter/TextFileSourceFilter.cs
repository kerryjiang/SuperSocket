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
        private HashSet<string> m_IpLibrary = new HashSet<string>();
        private DateTime m_LastUpdatedTime = DateTime.MinValue;
        
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
                m_FileSourceWatcher = new FileSystemWatcher(filePath);
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
        
        bool IsFileChanged(string filePath, out DateTime lastModifiedTime)
        {
            lastModifiedTime = File.GetLastWriteTime(filePath);
            return lastModifiedTime > m_LastUpdatedTime;
        }

        void HandleFileSourceWatcherChanged(object sender, FileSystemEventArgs e)
        {
            DateTime lastModifiedTime;
            if(!IsFileChanged(e.FullPath, out lastModifiedTime))
                return;
            
            this.ExecuteAsync(w => ProcessUpdatedFile(e.FullPath, lastModifiedTime));
        }

        void HandleFileSourceWatcherCreated(object sender, FileSystemEventArgs e)
        {
            DateTime lastModifiedTime;
            if(!IsFileChanged(e.FullPath, out lastModifiedTime))
                return;
            
            this.ExecuteAsync(w => ProcessUpdatedFile(e.FullPath, lastModifiedTime));
        }
        
        void ProcessUpdatedFile(string filePath, DateTime lastModifiedTime)
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
                m_LastUpdatedTime = lastModifiedTime;
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