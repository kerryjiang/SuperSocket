using System;
using SuperSocket.Common;

namespace SuperSocket.SocketService
{
	static partial class Program
	{
		static Program()
		{
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
		}

		static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogUtil.LogError(e.ExceptionObject as Exception);
		}
	}
}