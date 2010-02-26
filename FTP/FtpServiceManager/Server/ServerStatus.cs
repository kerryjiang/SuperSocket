using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.FtpManager;
using GiantSoft.FtpManagerCore;
using System.ServiceModel;
using GiantSoft.Common;
using GiantSoft.FtpServiceManager.Controls;

namespace GiantSoft.FtpServiceManager.Server
{
	public partial class ServerStatus : UserControl, ITabContent
	{
		private System.Timers.Timer m_RereshTimer = new System.Timers.Timer();

		public ServerStatus()
		{
			InitializeComponent();
		}

		#region IServerBinding Members

		public GiantSoft.FtpManagerCore.Model.FtpServerInfo Server { get; set; }

		#endregion

		private void ServerStatus_Load(object sender, EventArgs e)
		{
			GetServerStatus();
			m_RereshTimer.Interval = 5000;
			m_RereshTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_RereshTimer_Elapsed);
			m_RereshTimer.Enabled = false;
		}

		void m_RereshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			GetServerStatus();
		}

		private void GetServerStatus()
		{
			IStatusReporter status = null;

			try
			{
				status = ProxyFactory.CreateInstance<StatusReporterClient, IStatusReporter>(Server);
				this.lblCurrentConnectionCount.Text = status.GetCurrentConnectionCount().ToString();
				this.lblCurrentOnlineUserCount.Text = status.GetOnlineUserCount().ToString();
			}
			catch (FaultException exc)
			{
				LogUtil.LogError(exc);
			}
			catch (Exception exce)
			{
				LogUtil.LogError(exce);
			}
			finally
			{
				ProxyFactory.CloseProxy<IStatusReporter>(status);
			}
		}

		private void btnRefersh_Click(object sender, EventArgs e)
		{
			GetServerStatus();
		}

		private void ckbAutoRefresh_CheckedChanged(object sender, EventArgs e)
		{
			if (ckbAutoRefresh.Checked)
			{
				m_RereshTimer.Enabled = true;
			}
			else
			{
				m_RereshTimer.Enabled = false;
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			if (ckbAutoRefresh.Checked && !m_RereshTimer.Enabled)
				m_RereshTimer.Enabled = true;

			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			if (m_RereshTimer.Enabled)
				m_RereshTimer.Enabled = false;

			base.OnLostFocus(e);
		}


		#region ITabContent Members

		private readonly static object m_MenuClick = new object();

		public event MenuClickHandler MenuClick
		{
			add { this.Events.AddHandler(m_MenuClick, value); }
			remove { this.Events.RemoveHandler(m_MenuClick, value); }
		}

		#endregion
	}
}
