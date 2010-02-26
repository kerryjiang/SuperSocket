using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GiantSoft.Common.IO
{
	public class FileContentWatcher : IDisposable
	{
		private Dictionary<string, FileReadInfo> m_FileWatcherDict = new Dictionary<string, FileReadInfo>();

		private FileSystemWatcher m_Watcher;

		private object m_SyncRoot = new object();

		public FileContentWatcher(string path, string filter)
		{
			m_Watcher = new FileSystemWatcher(path, filter);
			m_Watcher.EnableRaisingEvents = false;
			m_Watcher.IncludeSubdirectories = true;
		}

		public void Start()
		{
			m_Watcher.Changed += new FileSystemEventHandler(m_Watcher_Changed);
			m_Watcher.Created += new FileSystemEventHandler(m_Watcher_Changed);
			m_Watcher.Renamed += new RenamedEventHandler(m_Watcher_Changed);
			m_Watcher.EnableRaisingEvents = true;
		}

		void m_Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			try
			{
				DateTime lastModifyTime = File.GetLastWriteTime(e.FullPath);
				long fileSize = (new FileInfo(e.FullPath)).Length;
				DateTime prevModifyTime = DateTime.MinValue;

				FileReadInfo readInfo;

				if (m_FileWatcherDict.TryGetValue(e.FullPath.ToLower(), out readInfo))
				{
					if (lastModifyTime <= readInfo.LastModifyTime)
					{
						return;
					}
					else
					{
						readInfo.LastModifyTime = DateTime.Now;

						if (fileSize > readInfo.PrevLength && readInfo.Handler != null)
						{
							StringBuilder appendContent = new StringBuilder();

							lock (readInfo.SyncRoot)
							{
								using (Stream stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
								{
									stream.Seek(readInfo.PrevLength, SeekOrigin.Begin);

									int shouldRead = (int)(fileSize - readInfo.PrevLength);
									int thisShouldRead = shouldRead;
									int thisRead = 0;

									byte[] buffer = new byte[1024];

									if (thisShouldRead > buffer.Length)
										thisShouldRead = buffer.Length;

									while ((thisRead = stream.Read(buffer, 0, thisShouldRead)) > 0)
									{
										appendContent.Append(Encoding.ASCII.GetString(buffer, 0, thisRead));
										thisShouldRead = shouldRead - thisRead;

										if (thisShouldRead <= 0)
											break;

										if (thisShouldRead > buffer.Length)
											thisShouldRead = buffer.Length;
									}

									stream.Close();
								}
							}

							readInfo.PrevLength = fileSize;

							readInfo.Handler.Invoke(this, new FileContentAppendEventArgs(appendContent.ToString()));
						}						
					}
				}				
			}
			catch (Exception exc)
			{
				LogUtil.LogError(exc);
			}
		}

		public void RegisterHandler(string filePath, FileContentAppendEventHandler handler)
		{
			FileReadInfo readInfo = new FileReadInfo();
			readInfo.PrevLength = 0;
			readInfo.LastModifyTime = DateTime.MinValue;
			readInfo.Handler = handler;

			lock (m_SyncRoot)
			{
				m_FileWatcherDict[filePath.ToLower()] = readInfo;
			}
		}

		public void RemoveHandler(string filePath)
		{
			lock (m_SyncRoot)
			{
				m_FileWatcherDict.Remove(filePath.ToLower());
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			m_FileWatcherDict.Clear();
			m_FileWatcherDict = null;

			if (m_Watcher != null)
			{
				m_Watcher.EnableRaisingEvents = false;

				m_Watcher.Changed -= new FileSystemEventHandler(m_Watcher_Changed);
				m_Watcher.Created -= new FileSystemEventHandler(m_Watcher_Changed);
				m_Watcher.Renamed -= new RenamedEventHandler(m_Watcher_Changed);

				m_Watcher.Dispose();
			}
		}

		#endregion
	}
}
