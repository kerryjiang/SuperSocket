using System;
using System.Net;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketBase.ConnectionFilter
{
	public class BlackListConnectionFilter : IConnectionFilter
	{
		public BlackListConnectionFilter(string name)
		{
			Name = name;
		}	

		#region IConnectionFilter implementation
		
		public bool AllowConnect(IPEndPoint remoteAddress)
		{
			throw new NotImplementedException();
		}

		public string Name { get; private set; }
		
		#endregion
	}
}

