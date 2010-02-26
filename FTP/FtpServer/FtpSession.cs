using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore;
using GiantSoft.SocketServiceCore.Command;
using GiantSoft.FtpService.Membership;
using GiantSoft.FtpService.Storage;
using System.IO;

namespace GiantSoft.FtpService
{
	public class FtpSession : SocketSession
	{
		public FtpSession() : base()
		{
		
		}
		
		private ICommandSource<FtpSession> m_CommandSource = null;
		
		private FtpContext m_Context = null;

		public override object CommandSource
		{
			set { m_CommandSource = value as ICommandSource<FtpSession>; }
		}
		
		public override void Start()
		{
			m_Context = new FtpContext();

			m_Context.TempDirectory = this.FtpServiceProvider.GetTempDirectory(this.SessionID);

			base.Start(m_Context);
		}

		protected override void ExecuteCommand(CommandInfo cmdInfo)
		{
			ICommand<FtpSession> command = m_CommandSource.GetCommandByName(cmdInfo.Name);

			if (command != null)
			{
				command.Execute(this, cmdInfo);				
			}
		}

		protected override void OnClose()
		{
			this.FtpServiceProvider.ClearTempDirectory(m_Context);

			FtpOnlineUsers.RemoveOnlineUser(this, m_Context.User);
			
			base.OnClose();
		}
		
		public FtpContext Context
		{
			get { return m_Context; }
		}

		internal void CloseDataConnection()
		{
			if(m_DataConn != null)
			{
				m_DataConn.Close();
				m_DataConn = null;
			}
		}

		protected override void SayWelcome()
		{
			SendResponse(m_Context, Resource.FTP_Welcome);
		}
		
		public void SendParameterError()
		{
			SendResponse(m_Context, Resource.InvalidArguments_501);
		}

		private FtpServer m_Server = null;

		public override void SetServer(IRunable server)
		{
			m_Server = server as FtpServer;
		}

		public FtpServer Server
		{
			get { return m_Server; }
		}

		public FtpServiceProviderBase FtpServiceProvider
		{
			get { return m_Server.FtpServiceProvider; }
		}


		private DataConnection m_DataConn = null;

		internal DataConnection DataConn
		{
			get { return m_DataConn; }
			set { m_DataConn = value; }
		}

        public void SendResponse(string message)
        {
            base.SendResponse(m_Context, message);
        }

        public void SendResponse(string message, params object[] paramValues)
        {
            base.SendResponse(m_Context, string.Format(message, paramValues));
        }

        protected override void HandleExceptionalError(Exception e)
        {
            SendResponse(Resource.UnknownError_450);
        }
    }
}
