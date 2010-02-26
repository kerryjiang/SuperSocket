using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.FtpManager;
using GiantSoft.FtpManagerCore;
using GiantSoft.Common;

namespace GiantSoft.FtpServiceManager.Users
{
	public partial class FrmResetPassword : Form, IServerBinding, IUserBinding
	{
		public FrmResetPassword()
		{
			InitializeComponent();
		}

		#region IServerBinding Members

		public GiantSoft.FtpManagerCore.Model.FtpServerInfo Server { get; set; }

		#endregion

		#region IUserBinding Members

		public long UserID { get; set; }

		#endregion

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (UserID <= 0)
			{
				throw new ArgumentException("UserID should be assigned");
			}

			string password = txbPassword.Text.Trim();
			string confirmPassword = txbConfirmPassword.Text.Trim();

			if (string.IsNullOrEmpty(password))
			{
				epResetPassword.SetError(txbPassword, "New password is required!");
				return;
			}

			if (string.IsNullOrEmpty(confirmPassword))
			{
				epResetPassword.SetError(txbConfirmPassword, "Confirm password is required!");
				return;
			}

			if (string.Compare(password, confirmPassword, true) != 0)
			{
				epResetPassword.SetError(txbConfirmPassword, "Confirm password does not match!");
				return;
			}


			IUserManager userManager = null;
			bool throwException = false;
			bool result = false;

			try
			{
				userManager = ProxyFactory.CreateInstance<UserManagerClient, IUserManager>(Server);
				result = userManager.ChangePassword(UserID, password);
			}
			catch (Exception exc)
			{
				LogUtil.LogError(exc);
				throwException = true;
			}
			finally
			{
				ProxyFactory.CloseProxy<IUserManager>(userManager);
			}

			if (throwException || !result)
			{
				MessageBox.Show("Failed to reset this user's password!");
			}
			else
			{
				MessageBox.Show("The password has been reset!");
			}
		}		
	}
}
