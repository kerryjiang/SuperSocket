using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GiantSoft.Common
{
	public class GiantFileSystemWatcher : IDisposable
	{
		private FileSystemWatcher m_Watcher;

		private Dictionary<string, DateTime> m_DictUpdateTime = new Dictionary<string, DateTime>();

		public GiantFileSystemWatcher(string path, string filter)
		{
			m_Watcher = new FileSystemWatcher(path, filter);
			m_Watcher.Error += new ErrorEventHandler(m_Watcher_Error);
			m_Watcher.EnableRaisingEvents = false;
			m_Watcher.IncludeSubdirectories = true;
		}

		void m_Watcher_Error(object sender, ErrorEventArgs e)
		{
			LogUtil.LogError(e.GetException());
		}

		public bool Start()
		{
			if (m_FileChangeHandler != null)
			{
				m_Watcher.Changed += new FileSystemEventHandler(m_Watcher_Changed);
				m_Watcher.Created += new FileSystemEventHandler(m_Watcher_Changed);
				m_Watcher.Renamed += new RenamedEventHandler(m_Watcher_Changed);
				m_Watcher.EnableRaisingEvents = true;
				return true;
			}
			else
			{
				return false;
			}
		}

		void m_Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			try
			{
				DateTime lastModifyTime = File.GetLastWriteTime(e.FullPath);
				DateTime prevModifyTime = DateTime.MinValue;

				if (m_DictUpdateTime.TryGetValue(e.FullPath.ToLower(), out prevModifyTime))
				{
					if (lastModifyTime <= prevModifyTime)
					{
						return;
					}
					else
					{
						m_DictUpdateTime[e.FullPath.ToLower()] = lastModifyTime;
					}
				}
				else
				{
					m_DictUpdateTime[e.FullPath.ToLower()] = lastModifyTime;
				}

				if (m_FileChangeHandler != null)
				{
					m_FileChangeHandler.Invoke(this, e);
				}
			}
			catch (Exception exc)
			{
				LogUtil.LogError(exc);
			}
		}

		private FileSystemEventHandler m_FileChangeHandler;

		public event FileSystemEventHandler FileChangeHandler
		{
			add { m_FileChangeHandler += value; }
			remove { m_FileChangeHandler -= value; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			m_DictUpdateTime.Clear();
			m_DictUpdateTime = null;
			m_Watcher.EnableRaisingEvents = false;
			if (m_FileChangeHandler != null)
			{
				m_Watcher.Changed -= new FileSystemEventHandler(m_Watcher_Changed);
				m_Watcher.Created -= new FileSystemEventHandler(m_Watcher_Changed);
				m_Watcher.Renamed -= new RenamedEventHandler(m_Watcher_Changed);
			}
			m_Watcher.Dispose();
		}

		#endregion
	}
}
