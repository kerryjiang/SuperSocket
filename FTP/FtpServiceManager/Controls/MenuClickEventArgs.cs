using System;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpServiceManager.Controls
{
	public class MenuClickEventArgs : EventArgs
	{
		public MenuClickEventArgs()
			: base()
		{

		}

		public MenuClickEventArgs(MenuFunctionName functionName, FtpServerInfo server)
		{
			FunctionName = functionName;
			Server = server;
		}

		public MenuClickEventArgs(MenuFunctionName functionName, FtpServerInfo server, object value)
		{
			FunctionName = functionName;
			Server = server;
			Value = value;
		}

		public MenuFunctionName FunctionName { get; private set; }

		public FtpServerInfo Server { get; private set; }

		public object Value { get; private set; }

	}

	public delegate void MenuClickHandler(object sender, MenuClickEventArgs e);
}
