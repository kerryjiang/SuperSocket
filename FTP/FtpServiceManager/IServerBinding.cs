using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GiantSoft.FtpManagerCore.Model;

namespace GiantSoft.FtpServiceManager
{
	public interface IServerBinding
	{
		FtpServerInfo Server { get; set; }
	}
}
