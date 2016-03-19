using System;
using System.Collections.Generic;
using System.IO;


namespace SuperSocket.ProtoBase.Encoder
{
    /// <summary>
    /// The object protocol encoder
    /// </summary>
    public interface IProtoObjectEncoder
    {
        /// <summary>
        /// Encode object
        /// </summary>
        /// <param name="output">the output buffer</param>
        /// <param name="target">the object to be encoded</param>
        void EncodeObject(IOutputBuffer output, object target);
    }
}
