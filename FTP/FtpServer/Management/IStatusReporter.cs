using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SuperSocket.FtpService.Management
{
	[ServiceContract]
	public interface IStatusReporter
	{
		[OperationContract]
		int GetCurrentConnectionCount();

		[OperationContract]
		int GetOnlineUserCount();

		[OperationContract]
		string Ping();
	}
}
