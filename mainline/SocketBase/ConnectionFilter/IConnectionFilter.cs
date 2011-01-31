using System;
using System.Net;

namespace SuperSocket.SocketBase.ConnectionFilter
{
	public interface IConnectionFilter
	{
		string Name { get; }
		
		bool AllowConnect(IPEndPoint remoteAddress);
	}
}

