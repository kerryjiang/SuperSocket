using System;
using System.Collections.Generic;
using System.IO;

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
        /// <param name="output">the output buffer</param>
        /// <param name="data">the binary data to be encoded</param>
        /// <returns>the output binary data</returns>
        void EncodeData(IOutputBuffer output, ArraySegment<byte> data);

        /// <summary>
        /// Encode the binary segments
        /// </summary>
        /// <param name="output">the output buffer</param>
        /// <param name="data">the binary segments to be encoded</param>
        /// <returns>the output binary data</returns>
        void EncodeData(IOutputBuffer output, IList<ArraySegment<byte>> data);

    }
}
