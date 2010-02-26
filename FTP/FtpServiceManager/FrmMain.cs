using System;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore;
using GiantSoft.FtpManagerCore.Model;
using GiantSoft.FtpServiceManager.Controls;
using GiantSoft.FtpServiceManager.Help;
using GiantSoft.FtpServiceManager.Server;
using GiantSoft.FtpServiceManager.Users;

namespace GiantSoft.FtpServiceManager
{
	public partial class FrmMain : Form
	{
		public FrmMain()
		{
			InitializeComponent();
			this.Activated += new EventHandler(FrmMain_Activated);
		}

		void FrmMain_Activated(object sender, EventArgs e)
		{
			if (FtpServerController.FtpServers.Count <= 0)
			{
				ShowConnectDialog();
			}
		}		

		private void FrmMain_Load(object sender, EventArgs e)
		{			
			leftTreeMenu.MenuClick += new MenuClickHandler(leftTreeMenu_LeftMenuClick);
		}

		void leftTreeMenu_LeftMenuClick(object sender, MenuClickEventArgs e)
		{
			control_MenuClick(sender, e);
		}

		private void ShowConnectDialog()
		{
			this.Activated -= new EventHandler(FrmMain_Activated);

			FrmConnect form = new FrmConnect();

			if (form.ShowDialog() == DialogResult.OK)
			{
				FtpServerController.RegisterFtpServer(form.NewFtpServer);
				leftTreeMenu.CreateServerNode(form.NewFtpServer);
				CreateTabPage<ServerStatus>("Server Status", form.NewFtpServer);
			}
		}


		private void CreateTabPage<T>(string title, FtpServerInfo server) where T : Control, ITabContent, new()
		{
			T control = new T();
			control.Server = server;
			control.MenuClick += new MenuClickHandler(control_MenuClick);
			control.Dock = DockStyle.Fill;
			if (scMain.Panel2.Controls.Count > 0)
			{
				scMain.Panel2.Controls.Clear();
			}
			scMain.Panel2.Controls.Add(control);
		}

		void control_MenuClick(object sender, MenuClickEventArgs e)
		{
			switch (e.FunctionName)
			{
				case (MenuFunctionName.UserAdmin):
					CreateTabPage<UserList>("User Admin", e.Server);
					break;
				case (MenuFunctionName.Setting):
					CreateTabPage<ServerSetting>("Server Setting", e.Server);
					break;
				case (MenuFunctionName.Status):
					CreateTabPage<ServerStatus>("Server Status", e.Server);
					break;
				case (MenuFunctionName.CreateUser):
					ShowDialogForm<FrmNewUser>(e.Server);
					break;
				case (MenuFunctionName.EditUser):
					ShowDialogForm<FrmUserProperty>(e.Server);
					break;
				case (MenuFunctionName.ResetPassword):
					ShowDialogForm<FrmResetPassword>(e.Server, (long)e.Value);
					break;
				default:
					break;
			}
		}

		private void ShowDialogForm<T>(FtpServerInfo server) where T : Form, IServerBinding, new()
		{
			T dialog = new T();
			dialog.Server = server;
			dialog.ShowDialog();
		}

		private void ShowDialogForm<T>(FtpServerInfo server, long userID) where T : Form, IServerBinding, IUserBinding, new()
		{
			T dialog = new T();
			dialog.Server = server;
			dialog.UserID = userID;
			dialog.ShowDialog();
		}

		private void aboutGiantSoftFtpManagerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FrmAboutUs aboutUs = new FrmAboutUs();
			aboutUs.ShowDialog();
		}
	}
}
