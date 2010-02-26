using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpServiceManager.Controls
{
	public partial class UserListContextMenuStrip : ContextMenuStripBase
	{
		public UserListContextMenuStrip()
		{
			InitializeComponent();
			this.Size = new System.Drawing.Size(60, 70);
			EventHandler clickHandler = new EventHandler(OnContextMenuClick);
			ToolStripButton menuItem = new ToolStripButton("New User...", null, clickHandler);
			menuItem.Size = new System.Drawing.Size(59, 22);
			menuItem.Tag = MenuFunctionName.CreateUser;
			this.Items.Add(menuItem);			
		}
	}
}
