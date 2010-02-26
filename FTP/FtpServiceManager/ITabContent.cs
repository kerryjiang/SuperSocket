using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GiantSoft.FtpServiceManager.Controls;

namespace GiantSoft.FtpServiceManager
{
	public interface ITabContent : IServerBinding
	{
		event MenuClickHandler MenuClick;
	}
}
