using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The interface for protocol encoder of binary data
    /// </summary>
    public interface IProtoDataEncoder
    {
        /// <summary>
        /// Encode the binary data
        /// </summary>
        /// <param name="data">the binary data to be encoded</param>
        /// <returns>the output binary data</returns>
        IList<ArraySegment<byte>> EncodeData(ArraySegment<byte> data);

        /// <summary>
        /// Encode the binary segments
        /// </summary>
        /// <param name="data">the binary segments to be encoded</param>
        /// <returns>the output binary data</returns>
        IList<ArraySegment<byte>> EncodeData(IList<ArraySegment<byte>> data);
    }
}
