namespace SuperSocket.Quic;

public sealed class QuicTransportOptions
{
    public int MaxBidirectionalStreamCount { get; set; } = 100;

    /// <summary>
    /// The maximum number of concurrent inbound uni-directional streams per connection.
    /// </summary>
    public int MaxUnidirectionalStreamCount { get; set; } = 10;
    
    /// <summary>
    /// Error code used when the stream needs to abort the read or write side of the stream internally.
    /// </summary>
    public long DefaultStreamErrorCode { get; set; }

    /// <summary>
    /// Error code used when an open connection is disposed
    /// </summary>
    public long DefaultCloseErrorCode { get; set; }

    /// <summary>Gets or sets the idle timeout for connections. The idle timeout is the time after which the connection will be closed.
    /// Default <see cref="F:System.TimeSpan.Zero" /> means underlying implementation default idle timeout.</summary>
    /// <returns>The idle timeout for connections. The default is <see cref="F:System.TimeSpan.Zero" />, which means that the default idle timeout of the underlying implementation is used.</returns>
    public int? IdleTimeout { get; set; } 
    
}