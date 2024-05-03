namespace SuperSocket.Quic;

public sealed class QuicTransportOptions
{
    public int MaxBidirectionalStreamCount { get; set; } = 100;

    /// <summary>
    /// The maximum number of concurrent inbound uni-directional streams per connection.
    /// </summary>
    public int MaxUnidirectionalStreamCount { get; set; } = 10;

    /// <summary>The maximum read size.</summary>
    public long MaxReadBufferSize { get; set; } = 1048576L;

    /// <summary>The maximum write size.</summary>
    public long MaxWriteBufferSize { get; set; } = 65536L;

    /// <summary>The maximum length of the pending connection queue.</summary>
    public int Backlog { get; set; } = 512;

    /// <summary>
    /// Error code used when the stream needs to abort the read or write side of the stream internally.
    /// </summary>
    public long DefaultStreamErrorCode { get; set; }

    /// <summary>Error code used when an open connection is disposed.</summary>
    public long DefaultCloseErrorCode { get; set; }
}