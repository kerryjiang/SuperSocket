using System.Net;
using SuperSocket.ProtoBase;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.Connection
{
    public class SslStreamPipeConnection<TPackageInfo> : StreamPipeConnection<TPackageInfo>, IConnectionWithRemoteCertificate
    {
        public SslStreamPipeConnection(SslStream stream, EndPoint remoteEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
            : this(stream, remoteEndPoint, null, pipelineFilter, options)
        {
        }

        public SslStreamPipeConnection(SslStream stream, EndPoint remoteEndPoint, EndPoint localEndPoint, IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
            : base(stream, remoteEndPoint, localEndPoint, pipelineFilter, options)
        {
            if (stream.IsAuthenticated || stream.IsMutuallyAuthenticated)
            {
                RemoteCertificate = stream.RemoteCertificate;
            }
        }

        public X509Certificate RemoteCertificate { get; }
    }
}
