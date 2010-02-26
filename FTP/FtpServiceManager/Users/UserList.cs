using System;
using System.ServiceModel;
using System.Windows.Forms;
using GiantSoft.Common;
using GiantSoft.FtpManagerCore.FtpManager;
using GiantSoft.FtpManagerCore;
using GiantSoft.FtpManagerCore.Model;
using GiantSoft.FtpServiceManager.Controls;

namespace GiantSoft.FtpServiceManager.Users
{
	public partial class UserList : UserControl, ITabContent
	{
		private UserListContextMenuStrip m_ContextMenuStrip;
		private UserItemContextMenuStrip m_RowContextMenuStrip;

		public UserList()
		{
			InitializeComponent();
			m_ContextMenuStrip = new UserListContextMenuStrip();
			//dgvUsers.ContextMenuStrip = m_ContextMenuStrip;
			dgvUsers.AutoGenerateColumns = false;
			dgvUsers.CellDoubleClick += new DataGridViewCellEventHandler(dgvUsers_CellDoubleClick);
			dgvUsers.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgvUsers_DataBindingComplete);
			dgvUsers.CellMouseClick += new DataGridViewCellMouseEventHandler(dgvUsers_CellMouseClick);
			m_RowContextMenuStrip = new UserItemContextMenuStrip();
		}

		void dgvUsers_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (e.RowIndex >= 0)
				{					
					m_RowContextMenuStrip.UserID = (long)dgvUsers.Rows[e.RowIndex].Cells[0].Value;
					m_RowContextMenuStrip.Show(MousePosition.X, MousePosition.Y);
				}
				else
				{
					m_ContextMenuStrip.Show(MousePosition.X, MousePosition.Y);
				}
			}
		}				

		void dgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			FtpUser user = dgvUsers.Rows[e.RowIndex].DataBoundItem as FtpUser;

			FrmUserProperty property = new FrmUserProperty();
			property.User = user;
			property.ShowDialog();
		}		

		protected override void OnLoad(EventArgs e)
		{
			IUserManager userManager = null;

			try
			{
				userManager = ProxyFactory.CreateInstance<UserManagerClient, IUserManager>(Server);
				dgvUsers.DataSource = userManager.GetAllUsers();				
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
				ProxyFactory.CloseProxy<IUserManager>(userManager);
			}

			base.OnLoad(e);
		}

		void dgvUsers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			if (dgvUsers.Rows.Count > 0)
			{
				dgvUsers.Rows[0].Selected = false;
				//for (int i = 0; i < dgvUsers.Rows.Count; i++)
				//{
				//    dgvUsers.Rows[i].ContextMenuStrip = m_RowContextMenuStrip;
				//    //dgvUsers.Rows[i].
				//}
			}
		}

		void DispatchEvent(MenuClickHandler handler)
		{
			m_ContextMenuStrip.ContextMenuClick += handler;
			m_RowContextMenuStrip.ContextMenuClick += handler;
		}

		#region ITabContent Members

		public FtpServerInfo Server { get; set; }

		private readonly static object m_MenuClick = new object();

		public event MenuClickHandler MenuClick
		{
			add { this.Events.AddHandler(m_MenuClick, value); DispatchEvent(value); }
			remove { this.Events.RemoveHandler(m_MenuClick, value); }
		}

		#endregion
	}
}

