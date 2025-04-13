using System.IO;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a connection that provides access to a stream.
    /// </summary>
    internal interface IStreamConnection
    {
        /// <summary>
        /// Gets the stream associated with the connection.
        /// </summary>
        Stream Stream { get; }
    }
}