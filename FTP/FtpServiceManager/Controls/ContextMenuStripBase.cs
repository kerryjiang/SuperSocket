using System;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpServiceManager.Controls
{
	public partial class ContextMenuStripBase : ContextMenuStrip, IServerBinding
	{
		public ContextMenuStripBase()
		{
			InitializeComponent();
		}

		#region IServerBinding Members

		public FtpServerInfo Server { get; set; }

		#endregion

		protected virtual void OnContextMenuClick(object sender, EventArgs e)
		{
			ToolStripItem menuItem = sender as ToolStripItem;
			OnContextMenuClick(this, new MenuClickEventArgs((MenuFunctionName)menuItem.Tag, Server));			
		}
		
		private readonly static object m_ContextMenuClick = new object();

		protected virtual void OnContextMenuClick(object sender, MenuClickEventArgs e)
		{
			MenuClickHandler handler = this.Events[m_ContextMenuClick] as MenuClickHandler;
			if (handler != null)
			{
				handler(sender, e);
			}
		}

		public event MenuClickHandler ContextMenuClick
		{
			add { this.Events.AddHandler(m_ContextMenuClick, value); }
			remove { this.Events.RemoveHandler(m_ContextMenuClick, value); }
		}		
	}
}
