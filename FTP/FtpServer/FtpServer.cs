using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using GiantSoft.Common;
using GiantSoft.FtpService.Management;
using GiantSoft.FtpService.Membership;
using GiantSoft.FtpService.Storage;
using GiantSoft.SocketServiceCore;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.IdentityModel.Claims;

namespace GiantSoft.FtpService
{
	public class FtpServer : SocketServer<FtpSession>
	{
		public FtpServer()
			: base()
		{

		}

		public FtpServer(IPEndPoint localEndPoint)
			: base(localEndPoint)
		{
		}

		private ServiceHost m_ManagerHost = null;

		protected override bool StartManagementService()
		{
			try
			{
				string addressRoot = "http://localhost:8000/";

				FtpManager manager = new FtpManager(this);

				WSHttpBinding customBiding = new WSHttpBinding();				
				customBiding.Security.Mode = SecurityMode.Message;
				customBiding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

				EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("GiantSocketServer");

				m_ManagerHost = new ServiceHost(manager, new Uri(addressRoot));

				ServiceEndpoint endPoint = m_ManagerHost.AddServiceEndpoint(typeof(IServerManager), customBiding, "");
				endPoint.Address = new EndpointAddress(new Uri(addressRoot), identity);
				endPoint = m_ManagerHost.AddServiceEndpoint(typeof(IUserManager), customBiding, "UserManager");
				endPoint.Address = new EndpointAddress(new Uri(addressRoot + "UserManager"), identity);
				endPoint = m_ManagerHost.AddServiceEndpoint(typeof(IStatusReporter), customBiding, "StatusReporter");
				endPoint.Address = new EndpointAddress(new Uri(addressRoot + "StatusReporter"), identity);

				ServiceMetadataBehavior metadataBehavior;
				metadataBehavior = m_ManagerHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
				if (metadataBehavior == null)
				{
					metadataBehavior = new ServiceMetadataBehavior();
					m_ManagerHost.Description.Behaviors.Add(metadataBehavior);
				}

				metadataBehavior.HttpGetEnabled = true;

				m_ManagerHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "MEX");

				if (ServerCredentials != null)
				{
					m_ManagerHost.Description.Behaviors.Add(ServerCredentials);
				}

				m_ManagerHost.Open();

				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);

				if (m_ManagerHost.State != CommunicationState.Closed && m_ManagerHost.State != CommunicationState.Created)
					m_ManagerHost.Close();

				m_ManagerHost = null;

				return false;
			}
		}

		public override void Stop()
		{
			base.Stop();

			if (m_ManagerHost != null && m_ManagerHost.State != CommunicationState.Closed)
			{
				m_ManagerHost.Close();
			}
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

		protected override bool Ready
		{
			get { return (FtpServiceProvider != null && FtpServiceProvider != null); }
		}
	}
}
