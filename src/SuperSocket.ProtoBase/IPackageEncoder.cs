using System.Buffers;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Defines an encoder for writing package information to a buffer.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public interface IPackageEncoder<in TPackageInfo>
    {
        /// <summary>
        /// Encodes a package into the specified buffer writer.
        /// </summary>
        /// <param name="writer">The buffer writer to write the encoded package to.</param>
        /// <param name="pack">The package to encode.</param>
        /// <returns>The number of bytes written to the buffer.</returns>
        int Encode(IBufferWriter<byte> writer, TPackageInfo pack);
    }
}