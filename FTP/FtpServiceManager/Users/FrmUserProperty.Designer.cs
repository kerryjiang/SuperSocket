namespace GiantSoft.FtpServiceManager.Users
{
	partial class FrmUserProperty
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tbGeneral = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.txbStorageRoot = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.nudMaxSpace = new System.Windows.Forms.NumericUpDown();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.nudMaxSpeed = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.nudMaxConnection = new System.Windows.Forms.NumericUpDown();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.cbxDisabled = new System.Windows.Forms.CheckBox();
			this.lblName = new System.Windows.Forms.Label();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.fbdStorageRoot = new System.Windows.Forms.FolderBrowserDialog();
			this.tabControl1.SuspendLayout();
			this.tbGeneral.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxSpace)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxConnection)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tbGeneral);
			this.tabControl1.Location = new System.Drawing.Point(2, 6);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(381, 339);
			this.tabControl1.TabIndex = 0;
			// 
			// tbGeneral
			// 
			this.tbGeneral.Controls.Add(this.tableLayoutPanel1);
			this.tbGeneral.Controls.Add(this.lblName);
			this.tbGeneral.Location = new System.Drawing.Point(4, 22);
			this.tbGeneral.Name = "tbGeneral";
			this.tbGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tbGeneral.Size = new System.Drawing.Size(373, 313);
			this.tbGeneral.TabIndex = 0;
			this.tbGeneral.Text = "General";
			this.tbGeneral.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txbStorageRoot, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.nudMaxSpace, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnBrowse, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.nudMaxSpeed, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label4, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.label5, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.nudMaxConnection, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 4);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 52);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(361, 255);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Storage Root:";
			// 
			// txbStorageRoot
			// 
			this.txbStorageRoot.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanel1.SetColumnSpan(this.txbStorageRoot, 2);
			this.txbStorageRoot.Location = new System.Drawing.Point(103, 3);
			this.txbStorageRoot.Name = "txbStorageRoot";
			this.txbStorageRoot.Size = new System.Drawing.Size(187, 20);
			this.txbStorageRoot.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Maxmium Speed:";
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 34);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Maxmium Space:";
			// 
			// nudMaxSpace
			// 
			this.nudMaxSpace.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.nudMaxSpace.Location = new System.Drawing.Point(103, 30);
			this.nudMaxSpace.Name = "nudMaxSpace";
			this.nudMaxSpace.Size = new System.Drawing.Size(62, 20);
			this.nudMaxSpace.TabIndex = 5;
			// 
			// btnBrowse
			// 
			this.btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnBrowse.Location = new System.Drawing.Point(296, 3);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(62, 20);
			this.btnBrowse.TabIndex = 4;
			this.btnBrowse.Tag = "==";
			this.btnBrowse.Text = "Browse";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// nudMaxSpeed
			// 
			this.nudMaxSpeed.Location = new System.Drawing.Point(103, 57);
			this.nudMaxSpeed.Name = "nudMaxSpeed";
			this.nudMaxSpeed.Size = new System.Drawing.Size(62, 20);
			this.nudMaxSpeed.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(171, 34);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(23, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "MB";
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(171, 61);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(33, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "KB/S";
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label6, 2);
			this.label6.Location = new System.Drawing.Point(3, 88);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(147, 13);
			this.label6.TabIndex = 9;
			this.label6.Text = "Max Concurrent Connections:";
			// 
			// nudMaxConnection
			// 
			this.nudMaxConnection.Location = new System.Drawing.Point(171, 84);
			this.nudMaxConnection.Name = "nudMaxConnection";
			this.nudMaxConnection.Size = new System.Drawing.Size(75, 20);
			this.nudMaxConnection.TabIndex = 10;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 4);
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Controls.Add(this.cbxDisabled, 0, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 111);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(355, 100);
			this.tableLayoutPanel2.TabIndex = 11;
			// 
			// cbxDisabled
			// 
			this.cbxDisabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cbxDisabled.AutoSize = true;
			this.cbxDisabled.Location = new System.Drawing.Point(3, 6);
			this.cbxDisabled.Name = "cbxDisabled";
			this.cbxDisabled.Size = new System.Drawing.Size(118, 17);
			this.cbxDisabled.TabIndex = 0;
			this.cbxDisabled.Text = "Account is disabled";
			this.cbxDisabled.UseVisualStyleBackColor = true;
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblName.Location = new System.Drawing.Point(17, 13);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(65, 24);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "Name";
			// 
			// btnApply
			// 
			this.btnApply.Enabled = false;
			this.btnApply.Location = new System.Drawing.Point(308, 351);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(75, 23);
			this.btnApply.TabIndex = 1;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(227, 351);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(146, 351);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// FrmUserProperty
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(386, 380);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmUserProperty";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "User Property";
			this.Load += new System.EventHandler(this.FrmUserProperty_Load);
			this.tabControl1.ResumeLayout(false);
			this.tbGeneral.ResumeLayout(false);
			this.tbGeneral.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxSpace)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMaxConnection)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tbGeneral;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txbStorageRoot;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.NumericUpDown nudMaxSpace;
		private System.Windows.Forms.NumericUpDown nudMaxSpeed;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.FolderBrowserDialog fbdStorageRoot;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown nudMaxConnection;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.CheckBox cbxDisabled;
	}
}