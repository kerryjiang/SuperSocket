using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GiantSoft.FtpServiceManager.Controls
{
	public partial class UserItemContextMenuStrip : ContextMenuStripBase, IUserBinding
	{
		public UserItemContextMenuStrip()
		{
			InitializeComponent();
			this.Size = new System.Drawing.Size(60, 70);
			EventHandler clickHandler = new EventHandler(OnContextMenuClick);
			ToolStripButton menuItem = new ToolStripButton("Edit User...", null, clickHandler);
			menuItem.Size = new System.Drawing.Size(59, 22);
			menuItem.Tag = MenuFunctionName.EditUser;
			this.Items.Add(menuItem);
			menuItem = new ToolStripButton("Delete User...", null, clickHandler);
			menuItem.Size = new System.Drawing.Size(59, 22);
			menuItem.Tag = MenuFunctionName.DeleteUser;
			this.Items.Add(menuItem);
			menuItem = new ToolStripButton("Reset Password...", null, clickHandler);
			menuItem.Size = new System.Drawing.Size(59, 22);
			menuItem.Tag = MenuFunctionName.ResetPassword;
			this.Items.Add(menuItem);
		}

		#region IUserBinding Members

		public long UserID { get; set; }

		#endregion

		protected override void OnContextMenuClick(object sender, EventArgs e)
		{
			ToolStripItem menuItem = sender as ToolStripItem;
			OnContextMenuClick(this, new MenuClickEventArgs((MenuFunctionName)menuItem.Tag, Server, UserID));
		}
	}
}
