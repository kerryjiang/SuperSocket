using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using NUnit.Framework;
using SuperSocket.SocketBase;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace SuperSocket.Test
{
    [TestFixture]
    public class SecureTcpSocketServerTest : SocketServerTest
    {
        protected override string DefaultServerConfig
        {
            get
            {
                return "SecureTestServer.config";
            }
        }

        protected override Stream GetSocketStream(System.Net.Sockets.Socket socket)
        {
            SslStream stream = new SslStream(new NetworkStream(socket), false, new RemoteCertificateValidationCallback(ValidateRemoteCertificate));
            stream.AuthenticateAsClient("supersocket");
            return stream;
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
