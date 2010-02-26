namespace GiantSoft.FtpServiceManager
{
	partial class FrmConnect
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblServerName = new System.Windows.Forms.Label();
			this.lblUserName = new System.Windows.Forms.Label();
			this.lblPassword = new System.Windows.Forms.Label();
			this.cbbServerName = new System.Windows.Forms.ComboBox();
			this.cbbUserName = new System.Windows.Forms.ComboBox();
			this.txbPassword = new System.Windows.Forms.TextBox();
			this.btnConnect = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.70091F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 71.29909F));
			this.tableLayoutPanel1.Controls.Add(this.lblServerName, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblUserName, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblPassword, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.cbbServerName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.cbbUserName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.txbPassword, 1, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(331, 106);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lblServerName
			// 
			this.lblServerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblServerName.AutoSize = true;
			this.lblServerName.Location = new System.Drawing.Point(3, 11);
			this.lblServerName.Name = "lblServerName";
			this.lblServerName.Size = new System.Drawing.Size(70, 13);
			this.lblServerName.TabIndex = 0;
			this.lblServerName.Text = "Server name:";
			// 
			// lblUserName
			// 
			this.lblUserName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblUserName.AutoSize = true;
			this.lblUserName.Location = new System.Drawing.Point(3, 46);
			this.lblUserName.Name = "lblUserName";
			this.lblUserName.Size = new System.Drawing.Size(61, 13);
			this.lblUserName.TabIndex = 1;
			this.lblUserName.Text = "User name:";
			// 
			// lblPassword
			// 
			this.lblPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblPassword.AutoSize = true;
			this.lblPassword.Location = new System.Drawing.Point(3, 81);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(53, 13);
			this.lblPassword.TabIndex = 2;
			this.lblPassword.Text = "Password";
			// 
			// cbbServerName
			// 
			this.cbbServerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cbbServerName.FormattingEnabled = true;
			this.cbbServerName.Location = new System.Drawing.Point(98, 7);
			this.cbbServerName.Name = "cbbServerName";
			this.cbbServerName.Size = new System.Drawing.Size(218, 21);
			this.cbbServerName.TabIndex = 3;
			// 
			// cbbUserName
			// 
			this.cbbUserName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cbbUserName.FormattingEnabled = true;
			this.cbbUserName.Location = new System.Drawing.Point(98, 42);
			this.cbbUserName.Name = "cbbUserName";
			this.cbbUserName.Size = new System.Drawing.Size(218, 21);
			this.cbbUserName.TabIndex = 4;
			// 
			// txbPassword
			// 
			this.txbPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txbPassword.Location = new System.Drawing.Point(98, 78);
			this.txbPassword.Name = "txbPassword";
			this.txbPassword.Size = new System.Drawing.Size(218, 20);
			this.txbPassword.TabIndex = 5;
			this.txbPassword.UseSystemPasswordChar = true;
			// 
			// btnConnect
			// 
			this.btnConnect.Enabled = false;
			this.btnConnect.Location = new System.Drawing.Point(76, 137);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(75, 23);
			this.btnConnect.TabIndex = 1;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(192, 137);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// FrmConnect
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(355, 172);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmConnect";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Connect to Ftp Server";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblServerName;
		private System.Windows.Forms.Label lblUserName;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.ComboBox cbbServerName;
		private System.Windows.Forms.ComboBox cbbUserName;
		private System.Windows.Forms.TextBox txbPassword;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnCancel;
	}
}