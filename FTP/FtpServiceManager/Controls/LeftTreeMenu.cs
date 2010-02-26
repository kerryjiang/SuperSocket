using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpServiceManager.Controls
{
	public partial class LeftTreeMenu : UserControl
	{
		public LeftTreeMenu()
		{
			InitializeComponent();
			tvLeftMenu.NodeMouseClick += new TreeNodeMouseClickEventHandler(tvLeftMenu_NodeMouseClick);
		}

		void tvLeftMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Node.Parent != null)
				{
					MenuFunctionName nodeType = (MenuFunctionName)e.Node.Tag;
					FtpServerInfo server = e.Node.Parent.Tag as FtpServerInfo;

					if (server != null)
					{
						OnLeftMenuClick(e.Node, new MenuClickEventArgs(nodeType, server));
					}
				}
			}
		}

		public void CreateServerNode(FtpServerInfo server)
		{
			TreeNode node = new TreeNode(server.Name, 0, 0);
			node.Tag = server;
			TreeNode subNode = new TreeNode("Setting", 4, 4);
			subNode.Tag = MenuFunctionName.Setting;
			node.Nodes.Add(subNode);
			subNode = new TreeNode("User Admin", 1, 1);
			subNode.Tag = MenuFunctionName.UserAdmin;
			UserAdminContextMenu contexMenu = new UserAdminContextMenu();
			contexMenu.ContextMenuClick += this.Events[m_LeftMenuClick] as MenuClickHandler;
			contexMenu.Server = server;
			subNode.ContextMenuStrip = contexMenu;
			node.Nodes.Add(subNode);
			subNode = new TreeNode("Server Status", 5, 5);
			subNode.Tag = MenuFunctionName.Status;
			node.Nodes.Add(subNode);			
			node.ExpandAll();
			tvLeftMenu.Nodes.Add(node);
		}

		private readonly static object m_LeftMenuClick = new object();

		protected virtual void OnLeftMenuClick(object sender, MenuClickEventArgs e)
		{
			MenuClickHandler handler = this.Events[m_LeftMenuClick] as MenuClickHandler;
			if (handler != null)
			{
				handler(sender, e);
			}
		}

		public event MenuClickHandler MenuClick
		{
			add { this.Events.AddHandler(m_LeftMenuClick, value); }
			remove { this.Events.RemoveHandler(m_LeftMenuClick, value); }
		}
	}
}
