using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpManagerCore
{
	public static class FtpServerController
	{
		private static List<FtpServerInfo> m_FtpServers = new List<FtpServerInfo>();

		public static List<FtpServerInfo> FtpServers
		{
			get { return m_FtpServers; }
			set { m_FtpServers = value; }
		}

		public static void RegisterFtpServer(FtpServerInfo server)
		{
			foreach(FtpServerInfo s in m_FtpServers)
			{
				if (string.Compare(s.Name, server.Name, true) == 0)
				{
					return;
				}
			}

			m_FtpServers.Add(server);
		}
	}
}
