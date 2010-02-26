using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using GiantSoft.FtpManagerCore.FtpManager;

namespace GiantSoft.FtpManagerCore.Model
{
	public class FtpServerInfo
	{
		private FtpServerInfo()
		{

		}

		public FtpServerInfo(string name)
		{
			Name = name;
			string defaultAddress = "http://" + name + ":8000/";
			EndpointIdentity identity = EndpointIdentity.CreateDnsIdentity("GiantSocketServer");
			DefaultEndpoint = new EndpointAddress(new Uri(defaultAddress), identity);
			m_EndpointLibrary[typeof(IStatusReporter)] = new EndpointAddress(new Uri(defaultAddress + "StatusReporter"), identity);
			m_EndpointLibrary[typeof(IUserManager)] = new EndpointAddress(new Uri(defaultAddress + "UserManager"), identity);
			m_EndpointLibrary[typeof(IServerManager)] = new EndpointAddress(new Uri(defaultAddress + "ServerManager"), identity);
		}

		public string Name { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public EndpointAddress DefaultEndpoint { get; set; }

		private Dictionary<Type, EndpointAddress> m_EndpointLibrary = new Dictionary<Type, EndpointAddress>();

		public EndpointAddress GetEndpointAddress<T>()
		{
			EndpointAddress address;

			if (m_EndpointLibrary.TryGetValue(typeof(T), out address))
			{
				return address;
			}
			else
			{
				return DefaultEndpoint;
			}
		}
	}
}
