using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GiantSoft.Common;
using GiantSoft.FtpManagerCore;

namespace GiantSoft.FtpServiceManager
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			LogUtil.Setup(new ELLogger());
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new FrmConnect());
			Application.Run(new FrmMain());
		}
	}
}
