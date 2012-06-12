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
            if (!string.IsNullOrEmpty(cerConfig.FilePath))
            {
                //To keep compatible with website hosting
                string filePath;

                if (Path.IsPathRooted(cerConfig.FilePath))
                    filePath = cerConfig.FilePath;
                else
                {
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cerConfig.FilePath);
                }

                return new X509Certificate2(filePath, cerConfig.Password);
            }
            else
            {
                var storeName = cerConfig.StoreName;
                if (string.IsNullOrEmpty(storeName))
                    storeName = "Root";

                var store = new X509Store(storeName);

                store.Open(OpenFlags.ReadOnly);

                var cert = store.Certificates.OfType<X509Certificate2>().Where(c =>
                    c.Thumbprint.Equals(cerConfig.Thumbprint, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                store.Close();

                return cert;
            }
        }
    }
}
