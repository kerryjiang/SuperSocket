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
        protected override IServerConfig DefaultServerConfig
        {
            get
            {
                return new ServerConfig
                {
                    Ip = "Any",
                    LogCommand = true,
                    MaxConnectionNumber = 100,
                    Mode = SocketMode.Tcp,
                    Name = "Secure Test Socket Server",
                    Port = 2012,
                    ClearIdleSession = true,
                    ClearIdleSessionInterval = 1,
                    IdleSessionTimeOut = 5,
                    Security = "Tls",
                    Certificate = new CertificateConfig
                    {
                        IsEnabled = true,
                        Password = "supersocket",
                        FilePath = "supersocket.pfx"
                    }
                };
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
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                Console.WriteLine("SSL Certificate Validation Error!");
                Console.WriteLine(sslPolicyErrors.ToString());
                Console.WriteLine("Chain status:");
                foreach (var s in chain.ChainStatus)
                {
                    Console.WriteLine("\t" + s.Status + " : " + s.StatusInformation);
                }
                return false;
            }

            return true;
        }
    }
}
