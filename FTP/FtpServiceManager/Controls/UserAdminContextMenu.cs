using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpServiceManager.Controls
{
	public partial class UserAdminContextMenu : ContextMenuStripBase
	{
		public UserAdminContextMenu()
		{
			InitializeComponent();
			this.Size = new System.Drawing.Size(50, 50);
			EventHandler clickHandler = new EventHandler(OnContextMenuClick);
			ToolStripButton menuItem = new ToolStripButton("New User...", null, clickHandler);
			menuItem.Size = new System.Drawing.Size(49, 22);
			menuItem.Tag = MenuFunctionName.CreateUser;
			this.Items.Add(menuItem);			
		}
	}
}
