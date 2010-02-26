using System;
using System.ServiceModel;
using System.ServiceModel.Security;
using GiantSoft.Common;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpManagerCore
{
	public static class ProxyFactory
	{
		private static WSHttpBinding m_CustomBiding;

		static ProxyFactory()
		{
			m_CustomBiding = new WSHttpBinding();
			m_CustomBiding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
			m_CustomBiding.Security.Mode = SecurityMode.Message;
		}

		public static I CreateInstance<S,I>(FtpServerInfo server) where I : class
			where S : I
		{
			ChannelFactory<I> chanelFactory = new ChannelFactory<I>(m_CustomBiding, server.GetEndpointAddress<I>());
			chanelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
			chanelFactory.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new MyX509Validator();
			chanelFactory.Credentials.UserName.UserName = server.UserName;
			chanelFactory.Credentials.UserName.Password = server.Password;
			return chanelFactory.CreateChannel();
		}

		public static void CloseProxy<I>(I instance)
		{
			ICommunicationObject proxy = instance as ICommunicationObject;

			if (proxy != null)
			{
				try
				{
					if (proxy.State != CommunicationState.Closed
							&& proxy.State != CommunicationState.Closing
							&& proxy.State != CommunicationState.Faulted)
					{
						proxy.Close();
					}
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
				}
			}
		}
	}
}
