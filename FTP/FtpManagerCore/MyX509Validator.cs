using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GiantSoft.FtpManagerCore
{
	public class MyX509Validator : X509CertificateValidator
	{
		public override void Validate(X509Certificate2 certificate)
		{
			// validate argument
			if (certificate == null)
				throw new ArgumentNullException("certificate");
	 
			// check if the name of the certifcate matches
			if (certificate.SubjectName.Name != "CN=GiantSocketServer")
				throw new SecurityTokenValidationException("Certificated was not issued by thrusted issuer");
		}
	}
}
