using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Channel
{
    public interface IChannelWithRemoteCertificate
    {
        X509Certificate RemoteCertificate { get; }
    }
}
