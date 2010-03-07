using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using SuperSocket.Common;
using SuperSocket.FtpService.Membership;
using SuperSocket.FtpService.Storage;
using SuperSocket.SocketServiceCore;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.IdentityModel.Claims;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.FtpService
{
    public class FtpServer : AppServer<FtpSession>
	{
		public FtpServer()
			: base()
		{

		}

		public override void Stop()
		{
			base.Stop();
		}

		private FtpServiceProviderBase m_FtpServiceProvider = null;

		public FtpServiceProviderBase FtpServiceProvider
		{
			get
			{
				if (m_FtpServiceProvider == null)
				{
					m_FtpServiceProvider = GetProviderByName("FtpServiceProvider") as FtpServiceProviderBase;
					m_FtpServiceProvider.Resource = Resource.ResourceManager;
				}

				return m_FtpServiceProvider;
			}
		}

		public override bool IsReady
		{
			get { return (FtpServiceProvider != null && FtpServiceProvider != null); }
		}
	}
}
