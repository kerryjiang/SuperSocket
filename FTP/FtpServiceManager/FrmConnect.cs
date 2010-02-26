using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GiantSoft.FtpManagerCore.Model;
using System.Threading;
using GiantSoft.FtpManagerCore.FtpManager;
using GiantSoft.Common;
using GiantSoft.FtpManagerCore;
using System.ServiceModel.Security;
using System.ServiceModel;

namespace GiantSoft.FtpServiceManager
{
	public partial class FrmConnect : Form
	{
		List<Control> m_RequiredControls = new List<Control>();

		public FrmConnect()
		{
			InitializeComponent();
			this.AcceptButton = this.btnConnect;
			this.cbbServerName.TextChanged += new EventHandler(userInput_Change);
			m_RequiredControls.Add(cbbServerName);
			this.cbbUserName.TextChanged += new EventHandler(userInput_Change);
			m_RequiredControls.Add(cbbUserName);
			this.txbPassword.TextChanged += new EventHandler(userInput_Change);
			m_RequiredControls.Add(txbPassword);
		}

		void userInput_Change(object sender, EventArgs e)
		{
			foreach (Control control in m_RequiredControls)
			{
				if (string.IsNullOrEmpty(control.Text.Trim()))
				{
					btnConnect.Enabled = false;
					return;
				}
			}

			btnConnect.Enabled = true;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		public FtpServerInfo NewFtpServer { get; private set; }

		private void btnConnect_Click(object sender, EventArgs e)
		{
			this.Enabled = false;

			FtpServerInfo server = new FtpServerInfo(cbbServerName.Text.Trim());
			server.UserName = cbbUserName.Text.Trim();
			server.Password = txbPassword.Text.Trim();

			IStatusReporter client = null;

			string response;

			bool connected = false;
			bool validHost = true;
			bool authenticated = true;

			try
			{
				client = ProxyFactory.CreateInstance<StatusReporterClient, IStatusReporter>(server);
				response = client.Ping();
				connected = true;
				authenticated = true;
			}
			catch (MessageSecurityException)
			{
				validHost = true;
				authenticated = false;
			}
			catch (EndpointNotFoundException)
			{
				validHost = false;
			}
			catch (Exception exc)
			{
				LogUtil.LogInfo("Logon exception type:" + exc.GetType().ToString());
				response = exc.Message;
				LogUtil.LogError(exc);
			}
			finally
			{
				if (client != null)
				{
					ProxyFactory.CloseProxy<IStatusReporter>(client);
				}
			}

			if (connected)
			{				
				NewFtpServer = server;
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				if (!validHost)
				{
					MessageBox.Show("The server cannot be connected!");
				}
				else if (!authenticated)
				{
					MessageBox.Show("Invalid username or password!");
				}

				this.Enabled = true;
			}
		}
	}
}
