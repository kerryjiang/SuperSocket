using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.FtpService.Membership;
using SuperSocket.FtpService.Storage;
using System.IO;
using System.Security.Authentication;

namespace SuperSocket.FtpService
{
    public class FtpSession : AppSession<FtpSession>
	{
		public FtpSession() : base()
		{
            m_Context = new FtpContext();
		}
		
		private FtpContext m_Context = null;

        public override SocketContext Context
        {
            get { return m_Context; }
        }

        public FtpContext FtpContext
        {
            get { return m_Context; }
        }

        protected override void OnInit()
        {
            m_Context.TempDirectory = this.FtpServiceProvider.GetTempDirectory(this.SessionID);
        }

		protected override void OnClosed()
		{
			this.FtpServiceProvider.ClearTempDirectory(m_Context);
			FtpOnlineUsers.RemoveOnlineUser(this, m_Context.User);
		}

        public override void Close()
        {
            CloseDataConnection();
            base.Close();
        }

		internal void CloseDataConnection()
		{
			if(m_DataConn != null)
			{
				m_DataConn.Close();
				m_DataConn = null;
			}
		}

		public override void SayWelcome()
		{
			SendResponse(Resource.FTP_Welcome);
		}
		
		public void SendParameterError()
		{
			SendResponse(Resource.InvalidArguments_501);
		}

		public FtpServer Server
		{
			get { return this.AppServer as FtpServer; }
		}

		public FtpServiceProviderBase FtpServiceProvider
		{
            get { return Server.FtpServiceProvider; }
		}


		private DataConnection m_DataConn = null;

		internal DataConnection DataConn
		{
			get { return m_DataConn; }
			set { m_DataConn = value; }
		}

        public override void HandleExceptionalError(Exception e)
        {
            SendResponse(Resource.UnknownError_450);
        }        
    }
}
