using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Config;


namespace SuperSocket.SocketServiceCore
{
	/// <summary>
	/// X509Certificate Manager
	/// </summary>
	public static class AuthenticationManager
	{
		private static X509Certificate certificate = null;

		public static bool Initialize(IGetCerticateConfig config)
		{
			ICertificateConfig cerConfig = config.GetCertificate();

			if (certificate == null)
			{
				lock (typeof(AuthenticationManager))
				{
					try
					{
						certificate = new X509Certificate2(cerConfig.CertificateFilePath, cerConfig.CertificatePassword);
						return true;
					}
					catch(Exception e)
					{
						LogUtil.LogError(e);
						return false;
					}
				}
			}
			
			return true;		
		}

		/// <summary>
		/// Gets the SSL/TLS certificate.
		/// </summary>
		/// <param name="config">The config.</param>
		/// <returns></returns>
		public static X509Certificate GetCertificate()
		{			
			return certificate;
		}
	}
}
