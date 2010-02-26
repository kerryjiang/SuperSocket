using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GiantSoft.SocketServiceCore.Config
{
	public interface ICredentialConfig
	{
		string UserName { get; }

		string Password { get; }
	}
}
