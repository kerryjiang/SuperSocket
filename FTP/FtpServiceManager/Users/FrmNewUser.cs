using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.Model;
using GiantSoft.FtpManagerCore.FtpManager;
using GiantSoft.FtpManagerCore;
using GiantSoft.Common;

namespace GiantSoft.FtpServiceManager.Users
{
	public partial class FrmNewUser : Form, IServerBinding
	{
		public FrmNewUser()
		{
			InitializeComponent();
		}

		#region IServerBinding Members

		public FtpServerInfo Server { get; set; }		

		#endregion

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			FtpUser user = new FtpUser();
			user.UserName = txbUserName.Text.Trim();
			user.Password = txbPassword.Text.Trim();
			user.Root = txbStorageRoot.Text.Trim();
			long maxSpace = 1024 * 1024;
			user.MaxSpace = maxSpace * (int)nudMaxSpace.Value;
			user.MaxUploadSpeed = (int)nudMaxUploadSpeed.Value;
			user.MaxDownloadSpeed = (int)nudMaxDownloadSpeed.Value;
			user.MaxThread = (int)nudMaxConnection.Value;

			IUserManager userManager = null;

			CreateUserResult result = CreateUserResult.UnknownError;

			try
			{
				userManager = ProxyFactory.CreateInstance<UserManagerClient, IUserManager>(Server);
				result = userManager.CreateFtpUser(user);
			}
			catch (Exception exc)
			{
				LogUtil.LogError(exc);
			}
			finally
			{
				ProxyFactory.CloseProxy<IUserManager>(userManager);
			}

			if (result == CreateUserResult.Success)
			{
				MessageBox.Show("Create user successfully!");
				this.Close();
			}
			else
			{
				MessageBox.Show("Unkonw error!");
			}
		}
	}
}
