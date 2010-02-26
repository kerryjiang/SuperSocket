using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.SocketServiceCore.Config
{
	public interface ICertificateConfig
	{
		string CertificateFilePath { get; }

		string CertificatePassword { get; }
	}
}
