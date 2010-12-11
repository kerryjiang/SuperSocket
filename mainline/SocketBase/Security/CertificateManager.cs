using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Security
{
    static class CertificateManager
    {
        internal static X509Certificate Initialize(ICertificateConfig cerConfig)
        {
            //To keep compatible with website hosting
            string filePath;

            if (Path.IsPathRooted(cerConfig.CertificateFilePath))
                filePath = cerConfig.CertificateFilePath;
            else
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cerConfig.CertificateFilePath);
            }

            return new X509Certificate2(filePath, cerConfig.CertificatePassword);
        }

        internal static void CreateCertificate(string commonName, ICertificateConfig cerConfig)
        {
            byte[] certificateData = Certificate.CreateSelfSignCertificatePfx(commonName, //host name
                DateTime.Now, //not valid before
                DateTime.Now.AddYears(5), //not valid after
                cerConfig.CertificatePassword);

            using (BinaryWriter binWriter = new BinaryWriter(File.Open(cerConfig.CertificateFilePath, FileMode.Create)))
            {
                binWriter.Write(certificateData);
                binWriter.Flush();
            }
        }
    }
}
