using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Connection
{
    public interface IConnectionWithRemoteCertificate
    {
        X509Certificate RemoteCertificate { get; }
    }
}
