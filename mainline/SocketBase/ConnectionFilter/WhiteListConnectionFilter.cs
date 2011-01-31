using System;
using System.Net;

namespace SuperSocket.SocketBase.ConnectionFilter
{
	public sealed class WhiteListConnectionFilter : IConnectionFilter
	{
		public WhiteListConnectionFilter(string name)
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

