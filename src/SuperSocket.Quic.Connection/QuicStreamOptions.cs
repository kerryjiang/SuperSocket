using System.Net.Quic;

#pragma warning disable CA2252
namespace SuperSocket.Quic.Connection
{
    public sealed class QuicStreamOptions
    {
        public bool ServerStream { get; set; }

        public QuicStreamType StreamType { get; set; }
    }
}