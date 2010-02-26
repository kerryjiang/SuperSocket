namespace GiantSoft.FtpServiceManager.Controls
{
	partial class LeftTreeMenu
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LeftTreeMenu));
			this.tvLeftMenu = new System.Windows.Forms.TreeView();
			this.ilTreeNode = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// tvLeftMenu
			// 
			this.tvLeftMenu.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tvLeftMenu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvLeftMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tvLeftMenu.ImageIndex = 0;
			this.tvLeftMenu.ImageList = this.ilTreeNode;
			this.tvLeftMenu.Indent = 19;
			this.tvLeftMenu.ItemHeight = 18;
			this.tvLeftMenu.Location = new System.Drawing.Point(0, 0);
			this.tvLeftMenu.Name = "tvLeftMenu";
			this.tvLeftMenu.SelectedImageIndex = 0;
			this.tvLeftMenu.Size = new System.Drawing.Size(150, 240);
			this.tvLeftMenu.TabIndex = 0;
			// 
			// ilTreeNode
			// 
			this.ilTreeNode.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilTreeNode.ImageStream")));
			this.ilTreeNode.TransparentColor = System.Drawing.Color.Transparent;
			this.ilTreeNode.Images.SetKeyName(0, "server.jpg");
			this.ilTreeNode.Images.SetKeyName(1, "users.jpg");
			this.ilTreeNode.Images.SetKeyName(2, "person.jpg");
			this.ilTreeNode.Images.SetKeyName(3, "host.jpg");
			this.ilTreeNode.Images.SetKeyName(4, "setting.jpg");
			this.ilTreeNode.Images.SetKeyName(5, "status.jpg");
			// 
			// LeftTreeMenu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tvLeftMenu);
			this.Name = "LeftTreeMenu";
			this.Size = new System.Drawing.Size(150, 240);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView tvLeftMenu;
		private System.Windows.Forms.ImageList ilTreeNode;
	}
}
