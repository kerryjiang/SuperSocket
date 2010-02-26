using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.FtpManager;

namespace GiantSoft.FtpServiceManager.Users
{
	public partial class FrmUserProperty : Form, IServerBinding
	{
		private FtpUser m_User = null;

		public FrmUserProperty()
		{
			InitializeComponent();
		}

		public FtpUser User
		{
			set { m_User = value; }
		}

		private void FrmUserProperty_Load(object sender, EventArgs e)
		{
			if (m_User != null)
			{
				lblName.Text = m_User.UserName;
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{

		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (fbdStorageRoot.ShowDialog() == DialogResult.OK)
			{
				txbStorageRoot.Text = fbdStorageRoot.SelectedPath;
			}
		}

		#region IServerBinding Members

		public GiantSoft.FtpManagerCore.Model.FtpServerInfo Server { get; set; }
		
		#endregion
	}
}
