using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Common
{
	public interface ILogger
	{
		void LogError(Exception e);

		void LogError(string title, Exception e);

		void LogError(string message);
		
		void LogDebug(string message);
		
		void LogInfo(string message);
	}
}
