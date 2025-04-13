using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Defines a decoder for extracting package information from a buffer.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPackageDecoder<out TPackageInfo>
    {
        /// <summary>
        /// Decodes a package from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the package data.</param>
        /// <param name="context">The context associated with the decoding process.</param>
        /// <returns>The decoded package.</returns>
        TPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context);
    }
}