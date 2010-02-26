namespace GiantSoft.FtpServiceManager
{
	partial class FrmMain
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
			this.msMain = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.howDoIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.reportABugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutGiantSoftFtpManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsMain = new System.Windows.Forms.ToolStrip();
			this.scMain = new System.Windows.Forms.SplitContainer();
			this.ilTabPages = new System.Windows.Forms.ImageList(this.components);
			this.leftTreeMenu = new GiantSoft.FtpServiceManager.Controls.LeftTreeMenu();
			this.msMain.SuspendLayout();
			this.scMain.Panel1.SuspendLayout();
			this.scMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// msMain
			// 
			this.msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.msMain.Location = new System.Drawing.Point(0, 0);
			this.msMain.Name = "msMain";
			this.msMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.msMain.Size = new System.Drawing.Size(792, 24);
			this.msMain.TabIndex = 0;
			this.msMain.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
			this.fileToolStripMenuItem.Text = "&System";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			// 
			// settingToolStripMenuItem
			// 
			this.settingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
			this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
			this.settingToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.settingToolStripMenuItem.Text = "&Tool";
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.howDoIToolStripMenuItem,
            this.reportABugToolStripMenuItem,
            this.aboutGiantSoftFtpManagerToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// howDoIToolStripMenuItem
			// 
			this.howDoIToolStripMenuItem.Name = "howDoIToolStripMenuItem";
			this.howDoIToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.howDoIToolStripMenuItem.Text = "Ho&w do I";
			// 
			// reportABugToolStripMenuItem
			// 
			this.reportABugToolStripMenuItem.Name = "reportABugToolStripMenuItem";
			this.reportABugToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.reportABugToolStripMenuItem.Text = "&Report a Bug";
			// 
			// aboutGiantSoftFtpManagerToolStripMenuItem
			// 
			this.aboutGiantSoftFtpManagerToolStripMenuItem.Name = "aboutGiantSoftFtpManagerToolStripMenuItem";
			this.aboutGiantSoftFtpManagerToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.aboutGiantSoftFtpManagerToolStripMenuItem.Text = "&About GiantSoft Ftp Manager";
			this.aboutGiantSoftFtpManagerToolStripMenuItem.Click += new System.EventHandler(this.aboutGiantSoftFtpManagerToolStripMenuItem_Click);
			// 
			// tsMain
			// 
			this.tsMain.Location = new System.Drawing.Point(0, 24);
			this.tsMain.Name = "tsMain";
			this.tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tsMain.Size = new System.Drawing.Size(792, 25);
			this.tsMain.TabIndex = 1;
			this.tsMain.Text = "toolStrip1";
			// 
			// scMain
			// 
			this.scMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.scMain.Location = new System.Drawing.Point(0, 49);
			this.scMain.Name = "scMain";
			// 
			// scMain.Panel1
			// 
			this.scMain.Panel1.BackColor = System.Drawing.SystemColors.Window;
			this.scMain.Panel1.Controls.Add(this.leftTreeMenu);
			// 
			// scMain.Panel2
			// 
			this.scMain.Panel2.BackColor = System.Drawing.SystemColors.Window;
			this.scMain.Size = new System.Drawing.Size(792, 524);
			this.scMain.SplitterDistance = 200;
			this.scMain.SplitterWidth = 3;
			this.scMain.TabIndex = 2;
			// 
			// ilTabPages
			// 
			this.ilTabPages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilTabPages.ImageStream")));
			this.ilTabPages.TransparentColor = System.Drawing.Color.Transparent;
			this.ilTabPages.Images.SetKeyName(0, "users.jpg");
			this.ilTabPages.Images.SetKeyName(1, "setting.jpg");
			this.ilTabPages.Images.SetKeyName(2, "server.jpg");
			this.ilTabPages.Images.SetKeyName(3, "person.jpg");
			this.ilTabPages.Images.SetKeyName(4, "host.jpg");
			// 
			// leftTreeMenu
			// 
			this.leftTreeMenu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.leftTreeMenu.Location = new System.Drawing.Point(0, 0);
			this.leftTreeMenu.Name = "leftTreeMenu";
			this.leftTreeMenu.Size = new System.Drawing.Size(196, 520);
			this.leftTreeMenu.TabIndex = 0;
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(792, 573);
			this.Controls.Add(this.scMain);
			this.Controls.Add(this.tsMain);
			this.Controls.Add(this.msMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.msMain;
			this.Name = "FrmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "GiantSoft Ftp Manager";
			this.Load += new System.EventHandler(this.FrmMain_Load);
			this.msMain.ResumeLayout(false);
			this.msMain.PerformLayout();
			this.scMain.Panel1.ResumeLayout(false);
			this.scMain.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip msMain;
		private System.Windows.Forms.ToolStrip tsMain;
		private System.Windows.Forms.SplitContainer scMain;
		private GiantSoft.FtpServiceManager.Controls.LeftTreeMenu leftTreeMenu;
		private System.Windows.Forms.ImageList ilTabPages;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem howDoIToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reportABugToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutGiantSoftFtpManagerToolStripMenuItem;
	}
}