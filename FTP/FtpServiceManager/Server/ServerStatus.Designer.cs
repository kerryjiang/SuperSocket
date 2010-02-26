namespace GiantSoft.FtpServiceManager.Server
{
	partial class ServerStatus
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
			if (m_RereshTimer.Enabled)
				m_RereshTimer.Enabled = false;
			m_RereshTimer.Dispose();

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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.lblCurrentOnlineUserCount = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblCurrentConnectionCount = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnRefersh = new System.Windows.Forms.Button();
			this.ckbAutoRefresh = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblCurrentOnlineUserCount, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblCurrentConnectionCount, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnRefersh, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.ckbAutoRefresh, 1, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(371, 159);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.ForeColor = System.Drawing.Color.Black;
			this.label4.Location = new System.Drawing.Point(3, 68);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(133, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "Current Online User Count:";
			// 
			// lblCurrentOnlineUserCount
			// 
			this.lblCurrentOnlineUserCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblCurrentOnlineUserCount.AutoSize = true;
			this.lblCurrentOnlineUserCount.ForeColor = System.Drawing.Color.Black;
			this.lblCurrentOnlineUserCount.Location = new System.Drawing.Point(153, 68);
			this.lblCurrentOnlineUserCount.Name = "lblCurrentOnlineUserCount";
			this.lblCurrentOnlineUserCount.Size = new System.Drawing.Size(13, 13);
			this.lblCurrentOnlineUserCount.TabIndex = 3;
			this.lblCurrentOnlineUserCount.Text = "0";
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(3, 38);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(118, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Current Connect Count:";
			// 
			// lblCurrentConnectionCount
			// 
			this.lblCurrentConnectionCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblCurrentConnectionCount.AutoSize = true;
			this.lblCurrentConnectionCount.ForeColor = System.Drawing.Color.Black;
			this.lblCurrentConnectionCount.Location = new System.Drawing.Point(153, 38);
			this.lblCurrentConnectionCount.Name = "lblCurrentConnectionCount";
			this.lblCurrentConnectionCount.Size = new System.Drawing.Size(13, 13);
			this.lblCurrentConnectionCount.TabIndex = 2;
			this.lblCurrentConnectionCount.Text = "0";
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Server Status:";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(153, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Healthy";
			// 
			// btnRefersh
			// 
			this.btnRefersh.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnRefersh.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRefersh.Location = new System.Drawing.Point(3, 93);
			this.btnRefersh.Name = "btnRefersh";
			this.btnRefersh.Size = new System.Drawing.Size(75, 23);
			this.btnRefersh.TabIndex = 6;
			this.btnRefersh.Text = "Refresh";
			this.btnRefersh.UseVisualStyleBackColor = true;
			this.btnRefersh.Click += new System.EventHandler(this.btnRefersh_Click);
			// 
			// ckbAutoRefresh
			// 
			this.ckbAutoRefresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.ckbAutoRefresh.AutoSize = true;
			this.ckbAutoRefresh.ForeColor = System.Drawing.Color.Black;
			this.ckbAutoRefresh.Location = new System.Drawing.Point(153, 96);
			this.ckbAutoRefresh.Name = "ckbAutoRefresh";
			this.ckbAutoRefresh.Size = new System.Drawing.Size(88, 17);
			this.ckbAutoRefresh.TabIndex = 8;
			this.ckbAutoRefresh.Text = "Auto Refresh";
			this.ckbAutoRefresh.UseVisualStyleBackColor = true;
			this.ckbAutoRefresh.CheckedChanged += new System.EventHandler(this.ckbAutoRefresh_CheckedChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.tableLayoutPanel1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(10);
			this.panel1.Size = new System.Drawing.Size(391, 179);
			this.panel1.TabIndex = 5;
			// 
			// ServerStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.panel1);
			this.Name = "ServerStatus";
			this.Size = new System.Drawing.Size(391, 179);
			this.Load += new System.EventHandler(this.ServerStatus_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblCurrentConnectionCount;
		private System.Windows.Forms.Label lblCurrentOnlineUserCount;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnRefersh;
		private System.Windows.Forms.CheckBox ckbAutoRefresh;

	}
}
