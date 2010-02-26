namespace GiantSoft.FtpServiceManager.Users
{
	partial class UserList
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
			this.headTitleBar = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.dgvUsers = new System.Windows.Forms.DataGridView();
			this.UserID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.MaxSpace = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.MaxUploadSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.MaxDownloadSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.StorageRoot = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.footControlBar = new System.Windows.Forms.Panel();
			this.btnCreate = new System.Windows.Forms.Button();
			this.headTitleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
			this.footControlBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// headTitleBar
			// 
			this.headTitleBar.AutoSize = false;
			this.headTitleBar.BackColor = System.Drawing.Color.White;
			this.headTitleBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.headTitleBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1});
			this.headTitleBar.Location = new System.Drawing.Point(0, 0);
			this.headTitleBar.Name = "headTitleBar";
			this.headTitleBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.headTitleBar.Size = new System.Drawing.Size(497, 43);
			this.headTitleBar.TabIndex = 2;
			this.headTitleBar.Text = "toolStrip1";
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Image = global::GiantSoft.FtpServiceManager.Properties.Resources.person;
			this.toolStripLabel1.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(110, 40);
			this.toolStripLabel1.Text = "User Management";
			// 
			// dgvUsers
			// 
			this.dgvUsers.AllowUserToAddRows = false;
			this.dgvUsers.AllowUserToDeleteRows = false;
			this.dgvUsers.AllowUserToOrderColumns = true;
			this.dgvUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvUsers.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgvUsers.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dgvUsers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dgvUsers.ColumnHeadersHeight = 20;
			this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dgvUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserID,
            this.UserName,
            this.MaxSpace,
            this.MaxUploadSpeed,
            this.MaxDownloadSpeed,
            this.StorageRoot});
			this.dgvUsers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvUsers.Location = new System.Drawing.Point(0, 43);
			this.dgvUsers.Name = "dgvUsers";
			this.dgvUsers.ReadOnly = true;
			this.dgvUsers.RowHeadersVisible = false;
			this.dgvUsers.RowTemplate.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.dgvUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvUsers.Size = new System.Drawing.Size(497, 107);
			this.dgvUsers.TabIndex = 1;
			// 
			// UserID
			// 
			this.UserID.DataPropertyName = "UserID";
			this.UserID.HeaderText = "UserID";
			this.UserID.Name = "UserID";
			this.UserID.ReadOnly = true;
			this.UserID.Visible = false;
			// 
			// UserName
			// 
			this.UserName.DataPropertyName = "UserName";
			this.UserName.HeaderText = "User Name";
			this.UserName.Name = "UserName";
			this.UserName.ReadOnly = true;
			// 
			// MaxSpace
			// 
			this.MaxSpace.DataPropertyName = "MaxSpace";
			this.MaxSpace.HeaderText = "Maxmium Space";
			this.MaxSpace.Name = "MaxSpace";
			this.MaxSpace.ReadOnly = true;
			// 
			// MaxUploadSpeed
			// 
			this.MaxUploadSpeed.DataPropertyName = "MaxUploadSpeed";
			this.MaxUploadSpeed.HeaderText = "Maxmium Upload Speed";
			this.MaxUploadSpeed.Name = "MaxUploadSpeed";
			this.MaxUploadSpeed.ReadOnly = true;
			// 
			// MaxDownloadSpeed
			// 
			this.MaxDownloadSpeed.DataPropertyName = "MaxDownloadSpeed";
			this.MaxDownloadSpeed.HeaderText = "Max Download Speed";
			this.MaxDownloadSpeed.Name = "MaxDownloadSpeed";
			this.MaxDownloadSpeed.ReadOnly = true;
			// 
			// StorageRoot
			// 
			this.StorageRoot.DataPropertyName = "Root";
			this.StorageRoot.HeaderText = "Storage Root";
			this.StorageRoot.Name = "StorageRoot";
			this.StorageRoot.ReadOnly = true;
			// 
			// footControlBar
			// 
			this.footControlBar.BackColor = System.Drawing.SystemColors.Control;
			this.footControlBar.Controls.Add(this.btnCreate);
			this.footControlBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.footControlBar.Location = new System.Drawing.Point(0, 112);
			this.footControlBar.Name = "footControlBar";
			this.footControlBar.Size = new System.Drawing.Size(497, 38);
			this.footControlBar.TabIndex = 3;
			// 
			// btnCreate
			// 
			this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCreate.Location = new System.Drawing.Point(10, 8);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new System.Drawing.Size(97, 23);
			this.btnCreate.TabIndex = 0;
			this.btnCreate.Text = "Create New User";
			this.btnCreate.UseVisualStyleBackColor = true;
			// 
			// UserList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.footControlBar);
			this.Controls.Add(this.dgvUsers);
			this.Controls.Add(this.headTitleBar);
			this.Name = "UserList";
			this.Size = new System.Drawing.Size(497, 150);
			this.headTitleBar.ResumeLayout(false);
			this.headTitleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
			this.footControlBar.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStrip headTitleBar;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.DataGridView dgvUsers;
		private System.Windows.Forms.DataGridViewTextBoxColumn UserID;
		private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
		private System.Windows.Forms.DataGridViewTextBoxColumn MaxSpace;
		private System.Windows.Forms.DataGridViewTextBoxColumn MaxUploadSpeed;
		private System.Windows.Forms.DataGridViewTextBoxColumn MaxDownloadSpeed;
		private System.Windows.Forms.DataGridViewTextBoxColumn StorageRoot;
		private System.Windows.Forms.Panel footControlBar;
		private System.Windows.Forms.Button btnCreate;
	}
}
