using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpServiceManager.Controls;

namespace GiantSoft.FtpServiceManager.Server
{
	public partial class ServerSetting : UserControl, ITabContent
	{
		public ServerSetting()
		{
			InitializeComponent();
		}

		#region ITabContent Members

		public GiantSoft.FtpManagerCore.Model.FtpServerInfo Server { get; set; }

		private readonly static object m_MenuClick = new object();

		public event MenuClickHandler MenuClick
		{
			add { this.Events.AddHandler(m_MenuClick, value); }
			remove { this.Events.RemoveHandler(m_MenuClick, value); }
		}

		#endregion
	}
}
