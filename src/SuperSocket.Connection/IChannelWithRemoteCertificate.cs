using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a connection that provides access to a remote certificate.
    /// </summary>
    public interface IConnectionWithRemoteCertificate
    {
        /// <summary>
        /// Gets the remote certificate associated with the connection.
        /// </summary>
        X509Certificate RemoteCertificate { get; }
    }
}
