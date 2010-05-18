using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using SuperSocket.SocketServiceCore;
using System.Xml.Serialization;


namespace SuperSocket.FtpService
{
	[DataContract]
	public class FtpUser : SocketUser
	{
		private object m_UserLock = new object();
	
		private long m_MaxSpace;

		[DataMember]
		public virtual long MaxSpace
		{
			get { return m_MaxSpace; }
			set { m_MaxSpace = value; }
		}

		private long m_UsedSpace;

		[DataMember]
		public virtual long UsedSpace
		{
			get { return m_UsedSpace; }
			set { m_UsedSpace = value; }
		}

		private int m_MaxThread;

		[DataMember]
        [XmlAttribute("MaxThread")]
		public virtual int MaxThread
		{
			get { return m_MaxThread; }
			set { m_MaxThread = value; }
		}

		private int m_MaxUploadSpeed;

		[DataMember]
		public virtual int MaxUploadSpeed
		{
			get { return m_MaxUploadSpeed; }
			set { m_MaxUploadSpeed = value; }
		}

		private int m_MaxDownloadSpeed;

		[DataMember]
		public virtual int MaxDownloadSpeed
		{
			get { return m_MaxDownloadSpeed; }
			set { m_MaxDownloadSpeed = value; }
		}

        private string m_Root;

		[DataMember]
        [XmlAttribute("Root")]
		public string Root
        {
            get { return m_Root; }
            set { m_Root = value; }
        }

		private int m_ThreadCount = 0;

		[DataMember]
		public int ThreadCount
		{
			get { return m_ThreadCount; }
			set { m_ThreadCount = value; }
		}
		
		public bool IncreaseThread()
		{
			if(m_MaxThread>0)
			{
				lock(m_UserLock)
				{
					if(m_ThreadCount >= m_MaxThread)
						return false;
					else
					{
						m_ThreadCount++;
						return true;
					}
				}
			}
			else
			{
				return true;
			}		
		}

		public int DecreaseThread()
		{
			lock (m_UserLock)
			{				
				m_ThreadCount--;
				return m_ThreadCount;			
			}			
		}
		
		public void ChangeUsedSpace(long space)
		{
			lock(m_UserLock)
			{
				m_UsedSpace += space;
			}
		}
	}
}
